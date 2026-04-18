using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Platform.Gateway.Security.Constants;
using Platform.Gateway.Security.Transformations;
using System.Security.Claims;

namespace Platform.Gateway.Extensions;

public static class AuthenticationServiceExtensions
{
    // Gom toàn bộ phần authentication/authorization của gateway vào một chỗ:
    // - Keycloak JWT authentication
    // - JWT options sau khi Keycloak đã cấu hình xong
    // - ClaimsTransformer để đổi tên claim cho dễ dùng trong code
    // - Authorization policy để bảo vệ route của gateway
    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Ở gateway, "xác thực" nghĩa là:
        // - đọc access token client gửi lên
        // - kiểm tra token có đúng issuer, signature, audience... hay không
        // - nếu hợp lệ thì ASP.NET tạo ra HttpContext.User
        //
        // Nhờ vậy các bước sau của gateway mới biết request này là của ai.

        // Đọc section "Keycloak" trong appsettings để cấu hình JWT bearer auth.
        services.AddKeycloakAuthentication(configuration);

        // PostConfigure được dùng để "bổ sung" cấu hình sau khi package Keycloak đã set sẵn phần cơ bản.
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            // User.Identity.Name sẽ lấy từ claim này.
            // Với Keycloak thì preferred_username thường đúng thứ ta muốn hiển thị nhất.
            options.TokenValidationParameters.NameClaimType = "preferred_username";

            // Nếu sau này có mapping role thì ASP.NET sẽ hiểu role theo kiểu ClaimTypes.Role.
            options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

            // Event này chỉ để hỗ trợ debug khi token sai/hết hạn/sai issuer...
            // Khi học microservice, đây là chỗ rất hữu ích để biết request bị fail từ bước auth hay từ business.
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("AUTH FAIL: " + context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

        // Sau khi xác thực token thành công, transformer sẽ thêm các claim nội bộ của gateway.
        // Đây là bước "chuẩn hóa dữ liệu user" trước khi route request đi tiếp.
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

        // Policy này đang được dùng trong file YARP config để buộc route phải có user hợp lệ mới được đi qua.
        // Nói cách khác: route nào gắn policy này thì client không thể gọi ẩn danh.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(GatewayAuthorizationPolicies.Authenticated, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
