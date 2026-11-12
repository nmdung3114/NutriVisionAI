using System.Text.Json;
using NutriVision.Application.Common;

namespace NutriVision.IntegrationTests;

[Collection("api")]
public class HabitTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")).DateTime);

    /// <summary>Ghi một bữa đã xác nhận: 1 thành phần 500 g nhập tay, giá trị /100 g = tổng ÷ 5.</summary>
    internal static async Task<Guid> LogMeal(ApiClient c, DateOnly ngay, string loai, decimal kcal, decimal dam = 20, decimal bot = 50, decimal beo = 10, decimal xo = 5, string ten = "Bữa test")
    {
        var r = await c.Post("/api/meals", new
        {
            loaiBuaAn = loai, ngayAn = ngay.ToString("yyyy-MM-dd"), tenMonAn = ten,
            thanhPhan = new[] { Comp(ten, 500, "g", null, new { caloTren100g = kcal / 5, chatDamTren100g = dam / 5, tinhBotTren100g = bot / 5, chatBeoTren100g = beo / 5, chatXoTren100g = xo / 5 }) },
        });
        Assert.True(r.Success, $"{r.Code} {r.Json}");
        return r.Data.GetProperty("id").GetGuid();
    }

    private static JsonElement Day(Resp cal, DateOnly d) =>
        cal.Data.GetProperty("ngay").EnumerateArray().Single(x => x.GetProperty("ngay").GetString() == d.ToString("yyyy-MM-dd"));

    [Fact]
    public async Task Calendar_Marks_Each_Day_Against_Target()
    {
        var (c, _) = await NewVerifiedUser(); // mục tiêu 2000 kcal
        var today = Today;
        var past = today.AddDays(-2);
        await LogMeal(c, today, "Trua", 1000);
        await LogMeal(c, today, "Toi", 950);   // 1950 = 97.5% → đạt chuẩn
        await LogMeal(c, past, "Sang", 900);   // 45% → thiếu

        var cal = await c.Get($"/api/dashboard/calendar?nam={today.Year}&thang={today.Month}");
        Assert.True(cal.Success);
        Assert.Equal(today.Day, cal.Data.GetProperty("ngay").GetArrayLength()); // không có ngày tương lai
        var t = Day(cal, today);
        Assert.Equal("DatChuan", t.GetProperty("trangThai").GetString());
        Assert.Equal(2, t.GetProperty("soBuaAn").GetInt32());
        Assert.Equal(1950m, t.GetProperty("calo").GetDecimal());

        var pastCal = await c.Get($"/api/dashboard/calendar?nam={past.Year}&thang={past.Month}");
        Assert.Equal("Thieu", Day(pastCal, past).GetProperty("trangThai").GetString());
        Assert.Equal(JsonValueKind.Null, Day(pastCal, past.AddDays(1)).GetProperty("trangThai").ValueKind);

        Assert.Equal(400, (await c.Get("/api/dashboard/calendar?nam=2026&thang=13")).Status);
        Assert.Equal(0, (await c.Get($"/api/dashboard/calendar?nam={today.Year + 1}&thang=1")).Data.GetProperty("ngay").GetArrayLength());
        Assert.Equal(401, (await (await Anonymous()).Get("/api/dashboard/calendar")).Status);
    }
}
