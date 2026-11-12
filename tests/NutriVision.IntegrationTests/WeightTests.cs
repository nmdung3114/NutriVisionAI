namespace NutriVision.IntegrationTests;

[Collection("api")]
public class WeightTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")).DateTime);
    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd");

    [Fact]
    public async Task Log_Upserts_Per_Day_Tracks_Change_And_Syncs_Profile()
    {
        var (c, _) = await NewVerifiedUser(); // hồ sơ 160 cm, 55 kg, mục tiêu tự đặt 2000
        var empty = (await c.Get("/api/weight")).Data;
        Assert.Equal(55m, empty.GetProperty("hienTai").GetDecimal()); // lấy từ hồ sơ khi chưa có nhật ký
        Assert.Equal(0, empty.GetProperty("danhSach").GetArrayLength());

        var t = Today;
        Assert.True((await c.Post("/api/weight", new { ngay = D(t.AddDays(-30)), canNang = 58 })).Success);
        Assert.True((await c.Post("/api/weight", new { ngay = D(t.AddDays(-7)), canNang = 56.5 })).Success);
        await c.Post("/api/weight", new { canNang = 56 });
        var r = await c.Post("/api/weight", new { canNang = 55.2, ghiChu = "  sau khi chạy bộ " }); // cùng ngày → cập nhật
        Assert.True(r.Success);

        var o = r.Data;
        Assert.Equal(3, o.GetProperty("danhSach").GetArrayLength());
        Assert.Equal(55.2m, o.GetProperty("hienTai").GetDecimal());
        Assert.Equal(58m, o.GetProperty("banDau").GetDecimal());
        Assert.Equal(-1.3m, o.GetProperty("thayDoi7Ngay").GetDecimal());
        Assert.Equal(-2.8m, o.GetProperty("thayDoi30Ngay").GetDecimal());
        Assert.Equal("sau khi chạy bộ", o.GetProperty("danhSach")[2].GetProperty("ghiChu").GetString());

        var profile = (await c.Get("/api/profile")).Data;
        Assert.Equal(55.2m, profile.GetProperty("canNang").GetDecimal());
        Assert.Equal(2000m, profile.GetProperty("mucTieuCalo").GetDecimal()); // mục tiêu tự đặt được giữ

        // Ghi bù ngày cũ không ghi đè hồ sơ
        await c.Post("/api/weight", new { ngay = D(t.AddDays(-3)), canNang = 70 });
        Assert.Equal(55.2m, (await c.Get("/api/profile")).Data.GetProperty("canNang").GetDecimal());

        Assert.Equal(400, (await c.Post("/api/weight", new { canNang = 5 })).Status);
        Assert.Equal(400, (await c.Post("/api/weight", new { ngay = D(t.AddDays(1)), canNang = 60 })).Status);

        var id = o.GetProperty("danhSach")[0].GetProperty("id").GetGuid();
        var (other, _) = await NewVerifiedUser();
        Assert.Equal(403, (await other.Delete($"/api/weight/{id}")).Status);
        Assert.True((await c.Delete($"/api/weight/{id}")).Success);
        Assert.Equal(404, (await c.Delete($"/api/weight/{id}")).Status);
    }

    [Fact]
    public async Task Default_Target_Follows_New_Weight()
    {
        var (c, _) = await NewVerifiedUser(withProfile: false);
        var p = await c.Put("/api/profile", new { hoTen = "A", ngaySinh = "1995-01-01", gioiTinh = "Nam", chieuCao = 175, canNang = 80, mucDoVanDong = "VuaPhai" });
        var before = p.Data.GetProperty("mucTieuCalo").GetDecimal();
        await c.Post("/api/weight", new { canNang = 75 });
        var after = (await c.Get("/api/profile")).Data;
        Assert.True(after.GetProperty("mucTieuCalo").GetDecimal() < before); // TDEE giảm theo cân nặng
        Assert.Equal(Math.Round(after.GetProperty("chiSoTDEE").GetDecimal(), 0, MidpointRounding.AwayFromZero), after.GetProperty("mucTieuCalo").GetDecimal());
    }
}
