using System.Security.Claims;

namespace Platform.Gateway.Extensions
{
    /// <summary>
    /// Extension methods bổ sung cho ClaimsIdentity.
    /// Dùng khi cần thêm claim vào identity một cách an toàn (không bị duplicate).
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Thêm một claim vào identity NẾU nó chưa tồn tại.
        /// - Kiểm tra value không rỗng (tránh thêm claim vô nghĩa).
        /// - Kiểm tra claim chưa có trong identity (tránh duplicate claim gây lỗi).
        /// Được dùng trong ClaimsTransformer để an toàn merge claim từ Keycloak vào.
        /// </summary>
        public static void AddIfNotExists(this ClaimsIdentity identity, string type, string? value)
        {
            if (!string.IsNullOrEmpty(value) && !identity.HasClaim(c => c.Type == type))
            {
                identity.AddClaim(new Claim(type, value));
            }
        }
    }
}
