using FluentValidation;
using NutriVision.Application.Common;
using NutriVision.Domain.Constants;

namespace NutriVision.Application.Modules.Meals;

public class NutritionPer100gInputValidator : AbstractValidator<NutritionPer100gInput>
{
    public NutritionPer100gInputValidator()
    {
        RuleFor(x => x.CaloTren100g).InclusiveBetween(0, 900).WithMessage("Calo trên 100g phải từ 0 đến 900.");
        RuleFor(x => x.ChatDamTren100g).InclusiveBetween(0, 100).WithMessage("Chất đạm trên 100g phải từ 0 đến 100.");
        RuleFor(x => x.TinhBotTren100g).InclusiveBetween(0, 100).WithMessage("Tinh bột trên 100g phải từ 0 đến 100.");
        RuleFor(x => x.ChatBeoTren100g).InclusiveBetween(0, 100).WithMessage("Chất béo trên 100g phải từ 0 đến 100.");
        RuleFor(x => x.ChatXoTren100g).InclusiveBetween(0, 100).WithMessage("Chất xơ trên 100g phải từ 0 đến 100.");
    }
}

public class ComponentInputValidator : AbstractValidator<ComponentInput>
{
    public ComponentInputValidator()
    {
        RuleFor(x => x.TenThanhPhan).NotEmpty().WithMessage("Tên thành phần không được để trống.").MaximumLength(200).WithMessage("Tên thành phần tối đa 200 ký tự.");
        RuleFor(x => x.KhauPhan).GreaterThan(0).WithMessage("Khẩu phần phải > 0.").LessThanOrEqualTo(9999).WithMessage("Khẩu phần quá lớn.");
        RuleFor(x => x.DonViTinh).NotEmpty().WithMessage("Vui lòng chọn đơn vị tính.").MaximumLength(50);
        RuleFor(x => x.NhapTay!).SetValidator(new NutritionPer100gInputValidator()).When(x => x.NhapTay is not null);
    }
}

public class MealUpsertRequestValidator : AbstractValidator<MealUpsertRequest>
{
    public MealUpsertRequestValidator(AppClock clock)
    {
        RuleFor(x => x.LoaiBuaAn).Must(LoaiBuaAn.IsValid).WithMessage("Loại bữa ăn không hợp lệ (Sang, Trua, Toi, Phu).");
        RuleFor(x => x.TenMonAn).Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("Tên món không được để trống.")
            .MaximumLength(200).WithMessage("Tên món tối đa 200 ký tự.");
        RuleFor(x => x.ThanhPhan).NotNull().WithMessage("Phải có ít nhất 1 thành phần.")
            .Must(l => l is { Count: > 0 }).WithMessage("Phải có ít nhất 1 thành phần.")
            .Must(l => l is null || l.Count <= 50).WithMessage("Tối đa 50 thành phần mỗi bữa.");
        RuleForEach(x => x.ThanhPhan).SetValidator(new ComponentInputValidator());
        RuleFor(x => x.NgayAn!.Value)
            .Must(d => d <= clock.Today).WithMessage("Ngày ăn không được ở tương lai.")
            .Must(d => d >= clock.Today.AddYears(-2)).WithMessage("Ngày ăn quá xa trong quá khứ.")
            .When(x => x.NgayAn.HasValue);
    }
}
