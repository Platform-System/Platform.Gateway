namespace Platform.Gateway.Security.Claims
{
    /// <summary>
    /// Định nghĩa tên các custom claim được Gateway thêm vào sau khi xác thực JWT.
    /// Mục đích: tránh hardcode string "userId", "email"... rải rác khắp nơi.
    /// Các claim này sẽ được dùng trong ClaimsTransformer và ocelot.json (AddHeadersToRequest).
    /// </summary>
    public static class CustomClaims
    {
        /// <summary>Claim chứa Keycloak UserId (lấy từ claim "sub" trong JWT).</summary>
        public const string UserId = "userId";

        /// <summary>Claim chứa email người dùng (lấy từ claim "email" trong JWT).</summary>
        public const string Email = "email";

        /// <summary>Claim chứa username (lấy từ claim "preferred_username" trong JWT của Keycloak).</summary>
        public const string Username = "username";
    }
}
