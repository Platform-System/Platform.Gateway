using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Platform.Gateway.Extensions;
using Platform.Gateway.Security.Transformations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// ============================================================
// PLATFORM GATEWAY - Điểm vào duy nhất của toàn bộ hệ thống
// ============================================================
// Vai trò:
//   1. Xác thực JWT token (do Keycloak cấp) trước khi cho request đi qua
//   2. Chuẩn hóa claims từ Keycloak → gắn vào header (X-User-Id, X-User-Email...)
//   3. Định tuyến (routing) request đến đúng microservice dựa trên ocelot.json
// ============================================================

// BƯỚC 1: Tắt bộ chuyển đổi claim mặc định của .NET
// Vấn đề: .NET tự động đổi tên claim theo chuẩn SOAP cũ (ví dụ: "sub" → ClaimTypes.NameIdentifier)
// Hậu quả: mất đi tên gốc từ Keycloak, gây lỗi khi đọc claim
// Giải pháp: Clear() để giữ nguyên tên claim gốc từ JWT (ví dụ: "sub", "email", "preferred_username")
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// BƯỚC 2: Đăng ký Keycloak Authentication
// Đọc config từ appsettings.json section "Keycloak" (auth-server-url, realm, resource...)
// Tự động cấu hình JWT Bearer: validate token, issuer, audience với Keycloak server
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// BƯỚC 3: Tinh chỉnh thêm JWT Bearer Options SAU KHI AddKeycloakAuthentication đã chạy
// Dùng PostConfigure để KHÔNG ghi đè hoàn toàn config của Keycloak, chỉ bổ sung thêm
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // Khai báo claim nào đại diện cho "tên user" (User.Identity.Name)
    // → dùng "preferred_username" thay vì "name" (đặc thù của Keycloak)
    options.TokenValidationParameters.NameClaimType = "preferred_username";

    // Khai báo claim nào đại diện cho "role" của user trong hệ thống
    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

    // Đăng ký các event hooks của JWT pipeline
    options.Events = new JwtBearerEvents
    {
        // Chạy khi xác thực token thất bại (token hết hạn, chữ ký sai, issuer không khớp...)
        // Dùng để debug — log lý do thất bại ra console
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("❌ AUTH FAIL: " + ctx.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// BƯỚC 4: Đăng ký ClaimsTransformer
// Chạy tự động sau khi JWT được xác thực thành công
// Nhiệm vụ: map claim Keycloak ("sub", "preferred_username"...) → custom claim ("userId", "username"...)
// Xem: Security/Transformations/ClaimsTransformer.cs
builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

// BƯỚC 5: Đăng ký Authorization
// Cho phép dùng [Authorize] attribute và policy-based authorization
builder.Services.AddAuthorization();

// BƯỚC 6: Load file cấu hình định tuyến của Ocelot
// ocelot.json định nghĩa: upstream URL (client gọi vào) → downstream URL (service thật)
builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,    // false = bắt buộc phải có file, thiếu là app crash ngay khi khởi động
    reloadOnChange: true // true = hot-reload, sửa ocelot.json không cần restart app
);

// BƯỚC 7: Đăng ký Ocelot vào DI container
builder.Services.AddOcelot();

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE - Thứ tự RẤT QUAN TRỌNG
// ============================================================

// Phải chạy trước UseAuthorization để xác thực token
// → Gọi ClaimsTransformer.TransformAsync() ở đây
app.UseAuthentication();

// Kiểm tra quyền truy cập dựa trên claims đã được xác thực ở bước trên
app.UseAuthorization();

// Kích hoạt Ocelot làm reverse proxy
// Ocelot sẽ đọc ocelot.json → forward request đến đúng downstream service
// Đồng thời gắn custom headers (X-User-Id, X-User-Email...) từ claims vào request
await app.UseOcelot();

app.Run();
