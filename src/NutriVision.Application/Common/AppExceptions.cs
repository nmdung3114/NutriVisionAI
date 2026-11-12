namespace NutriVision.Application.Common;

/// <summary>Mã lỗi nghiệp vụ trả về cho client (khớp docs/api-contract.md).</summary>
public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    public const string AccountTempLocked = "ACCOUNT_TEMP_LOCKED";
    public const string AccountDisabled = "ACCOUNT_DISABLED";
    public const string EmailExists = "EMAIL_EXISTS";
    public const string TokenInvalid = "TOKEN_INVALID";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string TokenUsed = "TOKEN_USED";
    public const string WrongPassword = "WRONG_PASSWORD";
    public const string SamePassword = "SAME_PASSWORD";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string SelfActionForbidden = "SELF_ACTION_FORBIDDEN";
    public const string InvalidImage = "INVALID_IMAGE";
    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string NutritionDataMissing = "NUTRITION_DATA_MISSING";
    public const string ProfileRequired = "PROFILE_REQUIRED";
    public const string RateLimited = "RATE_LIMITED";
    public const string ServerError = "SERVER_ERROR";
}

/// <summary>Lỗi nghiệp vụ có kiểm soát: Api chuyển thành HTTP status + envelope.</summary>
public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public object? Payload { get; }
    public IReadOnlyList<string>? Errors { get; }

    public AppException(string code, string message, int statusCode = 400, object? data = null, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Payload = data;
        Errors = errors;
    }
}

public sealed class NotFoundException(string message = "Không tìm thấy dữ liệu yêu cầu.")
    : AppException(ErrorCodes.NotFound, message, 404);

public sealed class ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác này.")
    : AppException(ErrorCodes.Forbidden, message, 403);

public sealed class ConflictException(string message, string code = ErrorCodes.Conflict)
    : AppException(code, message, 409);

public sealed class UnauthorizedAppException(string message = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.", string code = ErrorCodes.Unauthorized)
    : AppException(code, message, 401);

/// <summary>Lỗi dữ liệu đầu vào, kèm lỗi theo từng trường.</summary>
public sealed class ValidationAppException : AppException
{
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }

    public ValidationAppException(IReadOnlyDictionary<string, string[]> fieldErrors, string message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.")
        : base(ErrorCodes.Validation, message, 400,
            new { fieldErrors },
            fieldErrors.SelectMany(kv => kv.Value.Select(v => $"{kv.Key}: {v}")).ToList())
    {
        FieldErrors = fieldErrors;
    }

    public static ValidationAppException Single(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
