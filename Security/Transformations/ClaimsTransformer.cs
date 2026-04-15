using Microsoft.AspNetCore.Authentication;
using Platform.Gateway.Extensions;
using Platform.Gateway.Security.Claims;
using System.Security.Claims;

namespace Platform.Gateway.Security.Transformations
{
    /// <summary>
    /// ClaimsTransformer chạy tự động sau khi ASP.NET xác thực JWT thành công.
    /// 
    /// Mục đích: "chuẩn hóa" claim từ Keycloak sang tên claim nội bộ của hệ thống.
    /// 
    /// Vấn đề cần giải quyết:
    ///   - Keycloak trả về claim với tên chuẩn OpenID: "sub", "email", "preferred_username"
    ///   - Ocelot cần đọc claim theo tên ta định nghĩa (trong AddHeadersToRequest của ocelot.json)
    ///   - Để 2 phía đồng bộ → ta map sang CustomClaims ("userId", "email", "username")
    /// 
    /// Thứ tự chạy trong pipeline:
    ///   JWT validate → ClaimsTransformer.TransformAsync() → Ocelot forward request + gắn headers
    /// </summary>
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Nếu identity không hợp lệ thì trả về nguyên như cũ, không xử lý
            if (principal.Identity is not ClaimsIdentity identity)
                return Task.FromResult(principal);

            // Thêm các custom claim chuẩn hóa vào identity (dùng AddIfNotExists để tránh duplicate)
            identity.AddIfNotExists(CustomClaims.Username, principal.GetUsername()); // "preferred_username" → "username"
            identity.AddIfNotExists(CustomClaims.Email, principal.GetEmail());       // "email" → "email" (giữ nguyên tên nhưng gom về 1 chuẩn)
            identity.AddIfNotExists(CustomClaims.UserId, principal.GetUserId());     // "sub" → "userId"

            return Task.FromResult(principal);
        }
    }
}
