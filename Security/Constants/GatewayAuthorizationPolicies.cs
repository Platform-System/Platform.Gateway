namespace Platform.Gateway.Security.Constants;

/// <summary>
/// Tên các authorization policy dùng trong gateway.
/// 
/// Tách ra constant để tránh hardcode string policy trong code và file config.
/// </summary>
public static class GatewayAuthorizationPolicies
{
    /// <summary>Policy yêu cầu request phải có user đã xác thực.</summary>
    public const string Authenticated = "authenticated";
}
