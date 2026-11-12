using System.Text.Json;
using NutriVision.Api.Common;
using NutriVision.Application.Common;

namespace NutriVision.Api.Middleware;

/// <summary>Chuyển exception thành envelope lỗi; không rò rỉ chi tiết kỹ thuật (NFR-13).</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499; // client đóng kết nối
        }
        catch (AppException ex)
        {
            await Write(context, ex.StatusCode, ApiResponse.Fail(ex.Code, ex.Message, ex.Payload, ex.Errors));
        }
        catch (BadHttpRequestException ex)
        {
            var status = ex.StatusCode == 413 ? 413 : 400;
            var code = status == 413 ? ErrorCodes.FileTooLarge : ErrorCodes.Validation;
            await Write(context, status, ApiResponse.Fail(code, status == 413 ? "Tệp tải lên quá lớn." : "Yêu cầu không hợp lệ."));
        }
        catch (InvalidDataException)
        {
            await Write(context, 400, ApiResponse.Fail(ErrorCodes.Validation, "Dữ liệu gửi lên không hợp lệ."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi không xử lý được tại {Path}", context.Request.Path);
            await Write(context, 500, ApiResponse.Fail(ErrorCodes.ServerError,
                "Đã xảy ra lỗi máy chủ. Vui lòng thử lại sau.", null, env.IsDevelopment() ? [ex.Message] : null));
        }
    }

    private static async Task Write(HttpContext ctx, int status, object body)
    {
        if (ctx.Response.HasStarted) return;
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext ctx)
    {
        var h = ctx.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"] = "DENY";
        h["Referrer-Policy"] = "no-referrer";
        if (!ctx.Request.Path.StartsWithSegments("/swagger"))
            h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        return next(ctx);
    }
}
