namespace Platform.Gateway.Security.Constants;

/// <summary>
/// Tên các claim nội bộ mà gateway dùng sau khi chuẩn hóa token.
/// 
/// Đây không phải claim "gốc" từ Keycloak, mà là tên claim riêng của gateway
/// để code trong hệ thống đọc thống nhất hơn.
/// 
/// Có thể hiểu đơn giản:
/// - provider bên ngoài có "ngôn ngữ" của provider
/// - gateway đổi về "ngôn ngữ nội bộ" của hệ thống
/// </summary>
public static class GatewayClaimTypes
{
    /// <summary>Id nội bộ của user, được map từ claim "sub".</summary>
    public const string UserId = "userId";

    /// <summary>Email của user, map từ claim "email".</summary>
    public const string Email = "email";

    /// <summary>Username hiển thị, map từ claim "preferred_username".</summary>
    public const string Username = "username";
}
