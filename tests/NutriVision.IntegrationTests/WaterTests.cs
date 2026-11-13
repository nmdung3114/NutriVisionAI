namespace NutriVision.IntegrationTests;

[Collection("api")]
public class WaterTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")).DateTime);

    [Fact]
    public async Task Add_Accumulates_Undo_Clamps_And_Validates()
    {
        var (c, _) = await NewVerifiedUser(); // 55 kg → mục tiêu 1950 ml
        var s0 = (await c.Get("/api/water")).Data;
        Assert.Equal(0, s0.GetProperty("luongMl").GetInt32());
        Assert.Equal(1950, s0.GetProperty("mucTieuMl").GetInt32());
        Assert.Equal(7, s0.GetProperty("bayNgay").GetArrayLength());

        Assert.Equal(0, (await c.Post("/api/water", new { ml = -250 })).Data.GetProperty("luongMl").GetInt32()); // chưa có → không âm
        await c.Post("/api/water", new { ml = 250 });
        await c.Post("/api/water", new { ml = 500 });
        var s = (await c.Post("/api/water", new { ml = 1500 })).Data;
        Assert.Equal(2250, s.GetProperty("luongMl").GetInt32());
        Assert.Equal(115.4m, s.GetProperty("phanTram").GetDecimal());
        Assert.Equal(1, s.GetProperty("soNgayDatMucTieu7Ngay").GetInt32());
        Assert.Equal(1750, (await c.Post("/api/water", new { ml = -500 })).Data.GetProperty("luongMl").GetInt32()); // hoàn tác
        Assert.Equal(0, (await c.Post("/api/water", new { ml = -2000 })).Data.GetProperty("luongMl").GetInt32()); // không xuống dưới 0

        var yesterday = Today.AddDays(-1).ToString("yyyy-MM-dd");
        await c.Post("/api/water", new { ml = 300, ngay = yesterday });
        Assert.Equal(300, (await c.Get($"/api/water?ngay={yesterday}")).Data.GetProperty("luongMl").GetInt32());
        Assert.Equal(0, (await c.Get("/api/water")).Data.GetProperty("luongMl").GetInt32());

        Assert.Equal(400, (await c.Post("/api/water", new { ml = 0 })).Status);
        Assert.Equal(400, (await c.Post("/api/water", new { ml = 5000 })).Status);
        Assert.Equal(400, (await c.Post("/api/water", new { ml = 200, ngay = Today.AddDays(1).ToString("yyyy-MM-dd") })).Status);
        for (var i = 0; i < 5; i++) await c.Post("/api/water", new { ml = 2000 });
        Assert.Equal(400, (await c.Post("/api/water", new { ml = 2000 })).Status); // vượt 10 lít/ngày

        // dữ liệu tách biệt giữa người dùng
        var (other, _) = await NewVerifiedUser(withProfile: false);
        var o = (await other.Get("/api/water")).Data;
        Assert.Equal(0, o.GetProperty("luongMl").GetInt32());
        Assert.Equal(2000, o.GetProperty("mucTieuMl").GetInt32());
    }
}
