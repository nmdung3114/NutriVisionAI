namespace NutriVision.Api.Common;

/// <summary>Envelope phản hồi chuẩn (docs/api-contract.md §1).</summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Code { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string message = "Thành công") => new() { Success = true, Message = message, Data = data };
    public static ApiResponse<object> Ok(string message = "Thành công") => new() { Success = true, Message = message };

    public static ApiResponse<object> Fail(string code, string message, object? data = null, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Code = code, Message = message, Data = data, Errors = errors };
}
