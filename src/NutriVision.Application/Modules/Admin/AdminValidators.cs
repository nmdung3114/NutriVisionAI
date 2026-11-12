using FluentValidation;
using NutriVision.Domain.Constants;

namespace NutriVision.Application.Modules.Admin;

public class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    public ChangeStatusRequestValidator()
    {
        RuleFor(x => x.TrangThai).Must(t => t is TrangThaiTaiKhoan.Active or TrangThaiTaiKhoan.Locked)
            .WithMessage("Trạng thái chỉ có thể là ACTIVE hoặc LOCKED.");
        RuleFor(x => x.LyDo).MaximumLength(500).WithMessage("Lý do tối đa 500 ký tự.");
    }
}

public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    public ChangeRoleRequestValidator() =>
        RuleFor(x => x.VaiTro).Must(VaiTro.IsValid).WithMessage("Vai trò chỉ có thể là USER hoặc ADMIN.");
}
