namespace KuaforSepeti.Application.DTOs.Common;

/// <summary>
/// Generic API response wrapper.
/// </summary>
/// <typeparam name="T">Data type.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };

    // Aliases for backwards compatibility
    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => Ok(data, message);
    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null) => Fail(message, errors);
}

/// <summary>
/// Non-generic API response.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse Ok(string? message = null) => new() { Success = true, Message = message };
    public static ApiResponse Fail(string message, List<string>? errors = null) => new() { Success = false, Message = message, Errors = errors };

    // Aliases for backwards compatibility
    public static ApiResponse SuccessResponse(string? message = null) => Ok(message);
    public static ApiResponse FailureResponse(string message, List<string>? errors = null) => Fail(message, errors);
}

