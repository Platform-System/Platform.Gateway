using Platform.Gateway.Security.Constants;
using Yarp.ReverseProxy.Transforms;

namespace Platform.Gateway.Extensions;

public static class ReverseProxyServiceExtensions
{
    // Cấu hình YARP cho gateway:
    // - đọc file route riêng của từng service
    // - build reverse proxy từ section ReverseProxy
    // - thêm request transform để gắn user headers xuống downstream services
    public static IServiceCollection AddGatewayReverseProxy(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Tách config ra nhiều file giúp sau này thêm service mới mà không làm yarp.json quá dài.
        configuration
            .AddJsonFile("yarp.json", optional: false, reloadOnChange: true)
            .AddJsonFile("yarp.catalog.json", optional: false, reloadOnChange: true)
            .AddJsonFile("yarp.identity.json", optional: false, reloadOnChange: true);

        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(transformBuilderContext =>
            {
                // Transform này chạy trên từng request trước khi YARP forward sang service thật.
                transformBuilderContext.AddRequestTransform(transformContext =>
                {
                    var user = transformContext.HttpContext.User;
                    if (user?.Identity?.IsAuthenticated == true)
                    {
                        // Gateway không chỉ xác thực token rồi bỏ đó.
                        // Nó còn truyền user context qua header để downstream service có thể dùng ngay,
                        // ví dụ: audit log, lấy user id hiện tại, kiểm tra business rule...
                        SetForwardedHeader(transformContext, GatewayRequestHeaders.UserId, user.GetClaim(GatewayClaimTypes.UserId));
                        SetForwardedHeader(transformContext, GatewayRequestHeaders.UserEmail, user.GetClaim(GatewayClaimTypes.Email));
                        SetForwardedHeader(transformContext, GatewayRequestHeaders.UserName, user.GetClaim(GatewayClaimTypes.Username));
                    }

                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }

    private static void SetForwardedHeader(RequestTransformContext context, string headerName, string? value)
    {
        // Xóa trước để chắc chắn request outgoing chỉ có đúng một giá trị mới nhất.
        context.ProxyRequest.Headers.Remove(headerName);

        if (!string.IsNullOrWhiteSpace(value))
        {
            // TryAddWithoutValidation dùng được cho custom header vì ta chủ động kiểm soát nội dung ở đây.
            context.ProxyRequest.Headers.TryAddWithoutValidation(headerName, value);
        }
    }
}
