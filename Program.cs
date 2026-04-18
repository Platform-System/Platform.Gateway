using Platform.Gateway.Extensions;
using System.IdentityModel.Tokens.Jwt;

// Mặc định .NET có thể tự đổi tên một số claim JWT sang tên kiểu cũ của ASP.NET/SOAP.
// Ví dụ: claim "sub" có thể bị map sang NameIdentifier.
// Gateway này cần đọc đúng tên claim gốc từ Keycloak, nên ta tắt mapping mặc định.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Gateway trong project này đóng vai trò "cổng vào chung" cho client.
// Luồng tổng quát sẽ là:
// 1. Client gọi Gateway
// 2. Gateway xác thực JWT
// 3. Gateway chọn route phù hợp trong YARP
// 4. Gateway forward request sang microservice đích
// 5. Gateway gắn thêm một số header user để service phía sau dùng thuận tiện hơn

// Đăng ký phần xác thực:
// - đọc JWT do Keycloak cấp
// - chuẩn hóa claim sau khi xác thực xong
// - thêm policy để chặn route nếu user chưa đăng nhập
builder.Services.AddGatewayAuthentication(builder.Configuration);

// Đăng ký reverse proxy:
// - nạp cấu hình route/cluster từ các file yarp*.json
// - forward request đến microservice thật
// - gắn thêm user headers xuống downstream service
builder.Services.AddGatewayReverseProxy(builder.Configuration);

var app = builder.Build();

// 1. Xác thực token trước.
// Nếu token không hợp lệ thì request sẽ không đi tiếp tới service phía sau.
app.UseAuthentication();

// 2. Kiểm tra policy/authorization sau khi đã có user.
// Ví dụ route nào yêu cầu "authenticated" thì user bắt buộc phải đăng nhập.
app.UseAuthorization();

// 3. Chuyển request sang YARP để proxy đến service đích.
// Từ thời điểm này Gateway không tự xử lý business nữa,
// mà chỉ làm nhiệm vụ định tuyến và forward.
app.MapReverseProxy();

app.Run();
