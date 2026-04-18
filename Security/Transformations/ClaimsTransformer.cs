using Microsoft.AspNetCore.Authentication;
using Platform.Gateway.Extensions;
using Platform.Gateway.Security.Constants;
using System.Security.Claims;

namespace Platform.Gateway.Security.Transformations
{
    /// <summary>
    /// Chuyển claim chuẩn từ Keycloak sang tên claim nội bộ của gateway.
    /// 
    /// Ví dụ:
    /// - "sub" -> "userId"
    /// - "preferred_username" -> "username"
    /// 
    /// Làm vậy để phần còn lại của gateway không phải nhớ tên claim gốc của từng identity provider.
    /// </summary>
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Nếu request chưa được xác thực thì principal có thể không có ClaimsIdentity hợp lệ.
            if (principal.Identity is not ClaimsIdentity identity)
                return Task.FromResult(principal);

            // AddIfNotExists giúp tránh thêm trùng claim nếu pipeline chạy nhiều lần.
            identity.AddIfNotExists(GatewayClaimTypes.Username, principal.GetUsername());
            identity.AddIfNotExists(GatewayClaimTypes.Email, principal.GetEmail());
            identity.AddIfNotExists(GatewayClaimTypes.UserId, principal.GetUserId());

            return Task.FromResult(principal);
        }
    }
}
