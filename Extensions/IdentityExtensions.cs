using System.Security.Claims;

namespace Platform.Gateway.Extensions
{
    /// <summary>
    /// Helper để thêm claim vào ClaimsIdentity một cách an toàn.
    /// 
    /// Lý do cần helper riêng:
    /// - tránh thêm claim rỗng
    /// - tránh thêm trùng cùng một claim type nhiều lần
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Chỉ thêm claim khi:
        /// - value có dữ liệu
        /// - identity chưa có claim cùng type
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
