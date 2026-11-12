using NutriVision.Application.Common;
using NutriVision.Application.Modules.Foods;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Meals;

public sealed record ComputedComponent(ComponentInput Input, ThucPhamThamChieu? Food, decimal Gram, NutritionValues Nutrition);

public interface IComponentNutritionService
{
    /// <summary>
    /// Tra cứu dinh dưỡng và quy đổi khẩu phần cho danh sách thành phần (FR-28, FR-29).
    /// Ném ValidationAppException / NUTRITION_DATA_MISSING khi dữ liệu không hợp lệ; không bao giờ tự bịa giá trị (BR-30).
    /// allowMissing=true (bản nháp AI) cho phép thành phần chưa có dữ liệu với dinh dưỡng = 0 để người dùng xử lý tiếp.
    /// </summary>
    Task<IReadOnlyList<ComputedComponent>> ComputeAsync(IReadOnlyList<ComponentInput> inputs, bool allowMissing = false, CancellationToken ct = default);
}

public class ComponentNutritionService(IFoodCatalog catalog) : IComponentNutritionService
{
    public async Task<IReadOnlyList<ComputedComponent>> ComputeAsync(IReadOnlyList<ComponentInput> inputs, bool allowMissing = false, CancellationToken ct = default)
    {
        var foods = (await catalog.GetActiveAsync(ct)).ToDictionary(f => f.Id);
        var result = new List<ComputedComponent>(inputs.Count);
        var missing = new List<int>();
        var fieldErrors = new Dictionary<string, string[]>();

        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            ThucPhamThamChieu? food = null;
            if (input.ThucPhamThamChieuId is { } id && !foods.TryGetValue(id, out food))
            {
                fieldErrors[$"thanhPhan[{i}].thucPhamThamChieuId"] = ["Thực phẩm tham chiếu không tồn tại hoặc đã ngừng sử dụng."];
                continue;
            }

            var gram = NutritionCalculator.QuyDoiGram(input.KhauPhan, input.DonViTinh, food);
            if (gram is null)
            {
                fieldErrors[$"thanhPhan[{i}].donViTinh"] = [$"Không quy đổi được đơn vị '{input.DonViTinh}'."];
                continue;
            }
            if (gram is <= 0 or > 9999.99m)
            {
                fieldErrors[$"thanhPhan[{i}].khauPhan"] = ["Khối lượng quy đổi phải trong khoảng 0–9999 g."];
                continue;
            }

            NutritionValues per100;
            if (input.NhapTay is { } manual)
                per100 = new NutritionValues(manual.CaloTren100g, manual.ChatDamTren100g, manual.TinhBotTren100g, manual.ChatBeoTren100g, manual.ChatXoTren100g);
            else if (food is not null)
                per100 = food.DinhDuongTren100g;
            else
            {
                missing.Add(i);
                result.Add(new ComputedComponent(input, null, gram.Value, NutritionValues.Zero));
                continue;
            }

            result.Add(new ComputedComponent(input, food, gram.Value, NutritionCalculator.TinhTheoGram(per100, gram.Value)));
        }

        if (fieldErrors.Count > 0) throw new ValidationAppException(fieldErrors);

        if (missing.Count > 0 && !allowMissing)
        {
            var names = string.Join(", ", missing.Select(i => $"\"{inputs[i].TenThanhPhan}\""));
            throw new AppException(ErrorCodes.NutritionDataMissing,
                $"Chưa có dữ liệu dinh dưỡng cho: {names}. Hãy chọn thực phẩm gần đúng, nhập tay giá trị hoặc xóa thành phần này.",
                400, new { thanhPhanThieu = missing });
        }
        return result;
    }
}
