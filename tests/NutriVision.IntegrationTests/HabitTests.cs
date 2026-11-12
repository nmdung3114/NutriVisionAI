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

    [Fact]
    public async Task Streak_And_Badges_Are_Computed_From_Confirmed_Meals()
    {
        var (c, _) = await NewVerifiedUser(); // mục tiêu 2000 kcal → đạm mục tiêu 100 g
        var empty = (await c.Get("/api/dashboard/streak")).Data;
        Assert.Equal(0, empty.GetProperty("chuoiHienTai").GetInt32());
        Assert.All(empty.GetProperty("huyHieu").EnumerateArray(), b => Assert.False(b.GetProperty("datDuoc").GetBoolean()));

        var today = Today;
        // Hôm qua + 2 ngày trước: đủ 3 bữa chính, giàu đạm, nhiều xơ, đạt chuẩn calo
        foreach (var d in new[] { today.AddDays(-1), today.AddDays(-2) })
        {
            await LogMeal(c, d, "Sang", 500, dam: 30, xo: 10);
            await LogMeal(c, d, "Trua", 800, dam: 40, xo: 10);
            await LogMeal(c, d, "Toi", 700, dam: 35, xo: 10);
        }
        await LogMeal(c, today.AddDays(-5), "Phu", 200); // chuỗi cũ, bị đứt

        var s = (await c.Get("/api/dashboard/streak")).Data;
        Assert.Equal(2, s.GetProperty("chuoiHienTai").GetInt32()); // hôm nay chưa ghi nhưng chuỗi vẫn giữ
        Assert.False(s.GetProperty("homNayDaGhi").GetBoolean());
        Assert.Equal(3, s.GetProperty("tongNgayGhiNhan").GetInt32());
        Assert.Equal(7, s.GetProperty("tongBuaAn").GetInt32());

        var badges = s.GetProperty("huyHieu").EnumerateArray().ToDictionary(b => b.GetProperty("ma").GetString()!);
        Assert.True(badges["FIRST_MEAL"].GetProperty("datDuoc").GetBoolean());
        Assert.False(badges["STREAK_3"].GetProperty("datDuoc").GetBoolean());
        Assert.Equal(2, badges["STREAK_3"].GetProperty("tienDo").GetInt32());
        Assert.Equal(2, badges["ON_TARGET_7"].GetProperty("tienDo").GetInt32());
        Assert.Equal(2, badges["PROTEIN_5"].GetProperty("tienDo").GetInt32());
        Assert.Equal(2, badges["FIBER_5"].GetProperty("tienDo").GetInt32());
        Assert.Equal(2, badges["FULL_DAY_3"].GetProperty("tienDo").GetInt32());

        await LogMeal(c, today, "Sang", 400);
        var after = (await c.Get("/api/dashboard/streak")).Data;
        Assert.Equal(3, after.GetProperty("chuoiHienTai").GetInt32());
        Assert.True(after.GetProperty("huyHieu").EnumerateArray().Single(b => b.GetProperty("ma").GetString() == "STREAK_3").GetProperty("datDuoc").GetBoolean());
    }
}
