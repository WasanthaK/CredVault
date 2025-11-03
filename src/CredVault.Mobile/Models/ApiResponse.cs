namespace CredVault.Mobile.Models;

/// <summary>
/// Base API response wrapper
/// </summary>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Generic object response
/// </summary>
public class ObjectApiResponseDto : ApiResponseDto<object>
{
}

/// <summary>
/// Boolean response
/// </summary>
public class BooleanApiResponseDto : ApiResponseDto<bool>
{
}
