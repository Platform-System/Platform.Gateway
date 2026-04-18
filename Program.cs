using Platform.Gateway.Extensions;
using System.IdentityModel.Tokens.Jwt;

// Mặc định .NET có thể tự đổi tên một số claim JWT sang tên kiểu cũ của ASP.NET/SOAP.
// Ví dụ: claim "sub" có thể bị map sang NameIdentifier.
// Gateway này cần đọc đúng tên claim gốc từ Keycloak, nên ta tắt mapping mặc định.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthentication();

// 2. Kiểm tra policy/authorization sau khi đã có user.
app.UseAuthorization();

// 3. Chuyển request sang YARP để proxy đến service đích.
app.MapReverseProxy();

app.Run();
