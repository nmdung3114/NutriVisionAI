using FluentValidation;

namespace NutriVision.Application.Modules.Auth;

public static class PasswordPolicy
{
    public const string Message = "Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số.";

    public static bool IsValid(string? password) =>
        !string.IsNullOrEmpty(password) && password.Length >= 8 && password.Length <= 128
        && password.Any(char.IsLetter) && password.Any(char.IsDigit);

    public static IRuleBuilderOptions<T, string> MatKhauHopLe<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Vui lòng nhập mật khẩu.")
            .Must(IsValid).WithMessage(Message);
}
