using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using NutriVision.Application.Common;

namespace NutriVision.Api.Filters;

/// <summary>Tự động chạy FluentValidation cho mọi tham số action có validator đăng ký.</summary>
public class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var fieldErrors = new Dictionary<string, List<string>>();

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;
            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (services.GetService(validatorType) is not IValidator validator) continue;

            ValidationResult result = await validator.ValidateAsync(new ValidationContext<object>(arg));
            foreach (var e in result.Errors)
            {
                var key = ToCamel(e.PropertyName);
                if (!fieldErrors.TryGetValue(key, out var list)) fieldErrors[key] = list = [];
                if (!list.Contains(e.ErrorMessage)) list.Add(e.ErrorMessage);
            }
        }

        if (fieldErrors.Count > 0)
            throw new ValidationAppException(fieldErrors.ToDictionary(k => k.Key, k => k.Value.ToArray()));

        await next();
    }

    /// <summary>"ThanhPhan[0].KhauPhan" → "thanhPhan[0].khauPhan".</summary>
    private static string ToCamel(string path) =>
        string.Join('.', path.Split('.').Select(p => p.Length == 0 ? p : char.ToLowerInvariant(p[0]) + p[1..]));
}
