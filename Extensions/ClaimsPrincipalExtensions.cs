using System.Security.Claims;

namespace Platform.Gateway.Extensions
{
    /// <summary>
    /// Extension methods tiện ích để đọc claim từ ClaimsPrincipal (user đã xác thực).
    /// Mục đích: gom tất cả logic đọc claim vào một chỗ, tránh lặp code ở nhiều nơi.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Tìm giá trị của một claim theo type bất kỳ.
        /// Trả về null nếu không tìm thấy.
        /// </summary>
        public static string? GetClaim(this ClaimsPrincipal user, string type)
        => user?.Claims.FirstOrDefault(c => c.Type == type)?.Value;

        /// <summary>
        /// Lấy UserId của user đang đăng nhập.
        /// Ưu tiên lấy từ claim "sub" (standard JWT claim do Keycloak phát).
        /// Fallback sang ClaimTypes.NameIdentifier nếu "sub" không có
        /// (phòng trường hợp dùng provider khác không phải Keycloak).
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal user)
            => user.GetClaim("sub") ?? user.GetClaim(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Lấy email của user.
        /// Ưu tiên lấy từ claim "email" (Keycloak chuẩn OpenID Connect).
        /// Fallback sang ClaimTypes.Email (chuẩn ASP.NET).
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal user)
            => user.GetClaim("email") ?? user.GetClaim(ClaimTypes.Email);

        /// <summary>
        /// Lấy username hiển thị của user.
        /// "preferred_username" là claim đặc trưng của Keycloak, thường là username đăng nhập.
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal user)
            => user.GetClaim("preferred_username");
    }
}
