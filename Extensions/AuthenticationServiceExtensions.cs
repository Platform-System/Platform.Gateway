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
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

        // Policy này đang được dùng trong file YARP config để buộc route phải có user hợp lệ mới được đi qua.
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
