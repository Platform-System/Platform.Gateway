using System.Security.Claims;

namespace Platform.Gateway.Extensions
{
    /// <summary>
    /// Nhóm helper để đọc các claim thường dùng từ ClaimsPrincipal.
    /// 
    /// Mục tiêu là gom cách đọc claim vào một chỗ thay vì hardcode string ở nhiều file khác nhau.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Tìm claim theo type và lấy ra giá trị.
        /// Trả về null nếu claim không tồn tại.
        /// </summary>
        public static string? GetClaim(this ClaimsPrincipal user, string type)
        => user?.Claims.FirstOrDefault(c => c.Type == type)?.Value;

        /// <summary>
        /// Lấy user id của token hiện tại.
        /// 
        /// "sub" là claim chuẩn của JWT/OpenID Connect và thường là giá trị ổn định nhất để định danh user.
        /// Fallback sang NameIdentifier để code vẫn dùng được với provider khác nếu cần.
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal user)
            => user.GetClaim("sub") ?? user.GetClaim(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Lấy email của user.
        /// Ưu tiên claim "email" từ token chuẩn, fallback sang ClaimTypes.Email.
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal user)
            => user.GetClaim("email") ?? user.GetClaim(ClaimTypes.Email);

        /// <summary>
        /// Lấy username hiển thị của user từ claim đặc trưng của Keycloak.
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal user)
            => user.GetClaim("preferred_username");
    }
}
