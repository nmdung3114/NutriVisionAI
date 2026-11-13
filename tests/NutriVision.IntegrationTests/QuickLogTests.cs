using NutriVision.Application.Common;

namespace NutriVision.IntegrationTests;

[Collection("api")]
public class QuickLogTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")).DateTime);

    [Fact]
    public async Task Repeat_Copies_Components_To_Today_And_Updates_Dashboard()
    {
        var (c, _) = await NewVerifiedUser();
        var yesterday = Today.AddDays(-1);
        var src = await HabitTests.LogMeal(c, yesterday, "Sang", 450, ten: "Phở bò tái");

        var rep = await c.Post($"/api/meals/{src}/repeat", new { });
        Assert.True(rep.Success, rep.Code);
        var d = rep.Data;
        Assert.NotEqual(src, d.GetProperty("id").GetGuid());
        Assert.Equal(Today.ToString("yyyy-MM-dd"), d.GetProperty("ngayAn").GetString());
        Assert.Equal("Sang", d.GetProperty("loaiBuaAn").GetString());
        Assert.Equal("Phở bò tái", d.GetProperty("tenMonAn").GetString());
        Assert.True(d.GetProperty("daXacNhan").GetBoolean());
        Assert.False(d.GetProperty("taoTuAI").GetBoolean());
        Assert.Equal(450m, d.GetProperty("tongDinhDuong").GetProperty("calo").GetDecimal());
        Assert.Equal(1, d.GetProperty("thanhPhan").GetArrayLength());
        Assert.Equal(450m, (await c.Get("/api/dashboard")).Data.GetProperty("homNay").GetProperty("calo").GetDecimal());

        // đổi loại bữa + ngày; ngày tương lai bị từ chối
        var asSnack = await c.Post($"/api/meals/{src}/repeat", new { loaiBuaAn = "Phu", ngayAn = yesterday.ToString("yyyy-MM-dd") });
        Assert.Equal("Phu", asSnack.Data.GetProperty("loaiBuaAn").GetString());
        Assert.Equal(400, (await c.Post($"/api/meals/{src}/repeat", new { ngayAn = Today.AddDays(1).ToString("yyyy-MM-dd") })).Status);
        Assert.Equal(400, (await c.Post($"/api/meals/{src}/repeat", new { loaiBuaAn = "Khuya" })).Status);

        // bữa của người khác → 403; bản nháp → 409
        var (other, _) = await NewVerifiedUser();
        Assert.Equal(403, (await other.Post($"/api/meals/{src}/repeat", new { })).Status);
        Factory.Vision.Next = FakeVisionClient.Default;
        var draft = (await c.Scan(ApiClient.Png(), "a.png")).Data.GetProperty("id").GetGuid();
        Assert.Equal(409, (await c.Post($"/api/meals/{draft}/repeat", new { })).Status);
    }

    [Fact]
    public async Task Frequent_Groups_By_Normalized_Name_And_Ranks_By_Count()
    {
        var (c, _) = await NewVerifiedUser();
        Assert.Equal(0, (await c.Get("/api/meals/frequent")).Data.GetArrayLength());

        var t = Today;
        await HabitTests.LogMeal(c, t.AddDays(-3), "Trua", 600, ten: "Cơm tấm");
        await HabitTests.LogMeal(c, t.AddDays(-2), "Trua", 620, ten: "com tam ");   // cùng món, khác dấu/hoa thường
        var latest = await HabitTests.LogMeal(c, t.AddDays(-1), "Toi", 650, ten: "CƠM TẤM");
        await HabitTests.LogMeal(c, t, "Sang", 400, ten: "Bánh mì");
        await HabitTests.LogMeal(c, t.AddDays(-120), "Sang", 400, ten: "Xôi"); // ngoài 90 ngày

        var list = (await c.Get("/api/meals/frequent?limit=5")).Data.EnumerateArray().ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(3, list[0].GetProperty("soLan").GetInt32());
        Assert.Equal(latest, list[0].GetProperty("mauBuaAnId").GetGuid());
        Assert.Equal("Trua", list[0].GetProperty("loaiBuaAn").GetString()); // loại bữa thường gặp nhất
        Assert.Equal(650m, list[0].GetProperty("calo").GetDecimal());
        Assert.Equal("Bánh mì", list[1].GetProperty("tenMonAn").GetString());
    }
}
