using FluentValidation;
using NutriVision.Application.Common;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(AppClock clock)
    {
        RuleFor(x => x.HoTen).NotEmpty().WithMessage("Vui lòng nhập họ tên.").MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự.");
        RuleFor(x => x.GioiTinh).Must(GioiTinh.IsValid).WithMessage("Giới tính không hợp lệ (Nam hoặc Nu).");
        RuleFor(x => x.MucDoVanDong).Must(MucDoVanDong.IsValid).WithMessage("Mức độ vận động không hợp lệ.");
        RuleFor(x => x.ChieuCao)
            .InclusiveBetween(HealthMetricsCalculator.MinHeightCm, HealthMetricsCalculator.MaxHeightCm)
            .WithMessage("Chiều cao phải trong khoảng 50–200 cm.");
        RuleFor(x => x.CanNang)
            .InclusiveBetween(HealthMetricsCalculator.MinWeightKg, HealthMetricsCalculator.MaxWeightKg)
            .WithMessage("Cân nặng phải trong khoảng 10–150 kg.");
        RuleFor(x => x.NgaySinh)
            .Must(d => d != default && HealthMetricsCalculator.TinhTuoi(d, clock.Today) is >= HealthMetricsCalculator.MinAge and <= HealthMetricsCalculator.MaxAge)
            .WithMessage("Độ tuổi phải từ 10 đến 100.");
        RuleFor(x => x.MucTieuCalo!.Value)
            .InclusiveBetween(800m, 6000m).WithMessage("Mục tiêu calo phải trong khoảng 800–6000 kcal.")
            .When(x => x.MucTieuCalo.HasValue);
    }
}
