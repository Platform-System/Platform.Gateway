namespace Platform.Gateway.Security.Constants;

/// <summary>
/// Danh sách custom header mà gateway gửi thêm cho downstream services.
/// 
/// Những header này được lấy từ user đã xác thực ở gateway.
/// </summary>
public static class GatewayRequestHeaders
{
    /// <summary>Header chứa user id hiện tại.</summary>
    public const string UserId = "X-User-Id";

    /// <summary>Header chứa email của user hiện tại.</summary>
    public const string UserEmail = "X-User-Email";

    /// <summary>Header chứa username hiển thị của user hiện tại.</summary>
    public const string UserName = "X-User-Name";
}
