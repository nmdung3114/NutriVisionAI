using FluentValidation;

namespace NutriVision.Application.Modules.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.HoTen).NotEmpty().WithMessage("Vui lòng nhập họ tên.")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Vui lòng nhập email.")
            .MaximumLength(256).WithMessage("Email tối đa 256 ký tự.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");
        RuleFor(x => x.MatKhau).MatKhauHopLe();
        RuleFor(x => x.XacNhanMatKhau).Equal(x => x.MatKhau).WithMessage("Xác nhận mật khẩu không khớp.");
        RuleFor(x => x.DongYDieuKhoan).Equal(true).WithMessage("Bạn cần đồng ý với điều khoản sử dụng.");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator() => RuleFor(x => x.Token).NotEmpty().WithMessage("Thiếu mã xác thực.").MaximumLength(512);
}

public class EmailRequestValidator : AbstractValidator<EmailRequest>
{
    public EmailRequestValidator() =>
        RuleFor(x => x.Email).NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Email không đúng định dạng.").MaximumLength(256);
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Vui lòng nhập email.").MaximumLength(256);
        RuleFor(x => x.MatKhau).NotEmpty().WithMessage("Vui lòng nhập mật khẩu.").MaximumLength(128);
    }
}

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator() => RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Thiếu refresh token.").MaximumLength(512);
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Thiếu mã đặt lại mật khẩu.").MaximumLength(512);
        RuleFor(x => x.MatKhauMoi).MatKhauHopLe();
        RuleFor(x => x.XacNhanMatKhau).Equal(x => x.MatKhauMoi).WithMessage("Xác nhận mật khẩu không khớp.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.MatKhauHienTai).NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.").MaximumLength(128);
        RuleFor(x => x.MatKhauMoi).MatKhauHopLe();
        RuleFor(x => x.XacNhanMatKhau).Equal(x => x.MatKhauMoi).WithMessage("Xác nhận mật khẩu không khớp.");
    }
}
