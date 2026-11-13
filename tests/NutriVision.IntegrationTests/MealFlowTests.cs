using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using Xunit;

namespace NutriVision.IntegrationTests;

public class ProfileTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static object Profile(decimal h, decimal w, string dob = "1995-01-01", decimal? goal = null) =>
        new { hoTen = "Test", ngaySinh = dob, gioiTinh = "Nam", chieuCao = h, canNang = w, mucDoVanDong = "VuaPhai", mucTieuCalo = goal };

    [Fact] // AC-17..AC-21, BR-14..BR-17
    public async Task Profile_Validation_And_Recalculation()
    {
        var (c, _) = await NewVerifiedUser(withProfile: false);
        var empty = await c.Get("/api/profile");
        Assert.False(empty.Data.GetProperty("daDayDu").GetBoolean());
        Assert.Equal(2000m, empty.Data.GetProperty("mucTieuCalo").GetDecimal());

        Assert.Contains("50–200", (await c.Put("/api/profile", Profile(30, 60))).Json.GetProperty("errors")[0].GetString());
        Assert.Contains("10–150", (await c.Put("/api/profile", Profile(170, 5))).Json.GetProperty("errors")[0].GetString());
        Assert.Contains("10 đến 100", (await c.Put("/api/profile", Profile(170, 60, "2025-01-01"))).Json.GetProperty("errors")[0].GetString());

        var a = await c.Put("/api/profile", Profile(170, 60));
        Assert.True(a.Success);
        var b = await c.Put("/api/profile", Profile(170, 65));
        var da = a.Data; var db = b.Data;
        Assert.True(db.GetProperty("chiSoBMI").GetDecimal() > da.GetProperty("chiSoBMI").GetDecimal());
        Assert.Equal(da.GetProperty("chiSoBMR").GetDecimal() + 50m, db.GetProperty("chiSoBMR").GetDecimal()); // +5kg × 10
        Assert.Equal(Math.Round(db.GetProperty("chiSoTDEE").GetDecimal()), db.GetProperty("mucTieuCalo").GetDecimal());
        Assert.True(db.GetProperty("daDayDu").GetBoolean());

        var custom = await c.Put("/api/profile", Profile(170, 65, goal: 1800));
        Assert.Equal(1800m, custom.Data.GetProperty("mucTieuCalo").GetDecimal());
    }

    [Fact] // AC-22, BR-18
    public async Task Avatar_Upload_Rules()
    {
        var (noProfile, _) = await NewVerifiedUser(withProfile: false);
        Assert.Equal(ErrorCodes.ProfileRequired, (await Upload(noProfile, ApiClient.Png(), "a.png")).Code);

        var (c, _) = await NewVerifiedUser();
        Assert.Equal(ErrorCodes.InvalidImage, (await Upload(c, Encoding.UTF8.GetBytes("%PDF-1.4 fake"), "a.png")).Code);
        var big = new byte[5 * 1024 * 1024 + 10];
        Assert.Equal(413, (await Upload(c, big, "big.png")).Status);

        var ok = await Upload(c, ApiClient.Png(), "a.png");
        Assert.True(ok.Success);
        var url = ok.Data.GetProperty("anhDaiDienUrl").GetString()!;
        var img = await c.Send(HttpMethod.Get, url, auth: false);
        Assert.Equal(200, img.Status);
        Assert.Equal("image/png", img.Message.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, (await c.Send(HttpMethod.Get, url.Replace("sig=", "sig=00"), auth: false)).Status);
    }

    private static Task<Resp> Upload(ApiClient c, byte[] bytes, string name)
    {
        var form = new MultipartFormDataContent { { new ByteArrayContent(bytes), "file", name } };
        return c.Send(HttpMethod.Post, "/api/profile/avatar", content: form);
    }
}

public class MealFlowTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static async Task<Guid> FoodId(ApiClient c, string q)
    {
        var r = await c.Get($"/api/foods?q={Uri.EscapeDataString(q)}&take=1");
        return r.Data[0].GetProperty("id").GetGuid();
    }

    [Fact] // AC-23, AC-29, AC-30, AC-31, AC-36, AC-39, AC-47, AC-49, BR-26, BR-31
    public async Task Scan_Draft_Confirm_Dashboard_History()
    {
        Factory.Vision.Next = FakeVisionClient.Default;
        var (c, _) = await NewVerifiedUser();

        var scan = await c.Scan(ApiClient.Png(), "com.png", "Trua");
        Assert.True(scan.Success, scan.Code);
        var d = scan.Data;
        var id = d.GetProperty("id").GetGuid();
        Assert.False(d.GetProperty("daXacNhan").GetBoolean());
        Assert.False(d.GetProperty("aiThatBai").GetBoolean());
        Assert.Equal("Cơm tấm sườn", d.GetProperty("tenMonAn").GetString());
        Assert.Equal(0.92m, d.GetProperty("doTinCay").GetDecimal());
        var comps = d.GetProperty("thanhPhan").EnumerateArray().ToList();
        Assert.Equal(3, comps.Count);
        var rice = comps.Single(x => x.GetProperty("tenThanhPhan").GetString() == "Cơm trắng");
        Assert.Equal("Cơm trắng gạo tẻ", rice.GetProperty("thamChieu").GetProperty("tenThucPham").GetString());
        Assert.Equal(260m, rice.GetProperty("luongCalo").GetDecimal()); // 200g × 130/100
        var unknown = comps.Single(x => x.GetProperty("tenThanhPhan").GetString() == "Món lạ hoắc");
        Assert.True(unknown.GetProperty("thieuDuLieuDinhDuong").GetBoolean()); // BR-30
        Assert.Equal(3, d.GetProperty("aiGoc").GetProperty("thanhPhan").GetArrayLength());
        Assert.NotNull(d.GetProperty("hinhAnhUrl").GetString());

        // AC-30: bản nháp chưa vào dashboard
        var dash0 = await c.Get("/api/dashboard");
        Assert.Equal(0m, dash0.Data.GetProperty("homNay").GetProperty("calo").GetDecimal());
        Assert.False(dash0.Data.GetProperty("coDuLieu").GetBoolean());

        // AC-32/33/34: validation khi xác nhận
        Assert.Equal(400, (await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Trua", tenMonAn = "X", thanhPhan = Array.Empty<object>() })).Status);
        var neg = await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Trua", tenMonAn = "X", thanhPhan = new[] { Comp("Cơm", -100) } });
        Assert.Contains("> 0", neg.Json.GetProperty("errors")[0].GetString());
        Assert.Equal(400, (await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Trua", tenMonAn = " ", thanhPhan = new[] { Comp("Cơm", 100) } })).Status);
        // AC-37: thành phần không có dữ liệu → NUTRITION_DATA_MISSING
        var missing = await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Trua", tenMonAn = "Cơm tấm", thanhPhan = new[] { Comp("Món lạ hoắc", 30) } });
        Assert.Equal(ErrorCodes.NutritionDataMissing, missing.Code);
        Assert.Equal(0, missing.Data.GetProperty("thanhPhanThieu")[0].GetInt32());

        var riceId = await FoodId(c, "cơm trắng");
        var ribId = await FoodId(c, "sườn heo nướng");
        var confirm = await c.Post($"/api/meals/{id}/confirm", new
        {
            loaiBuaAn = "Trua", tenMonAn = "Cơm tấm sườn nướng",
            thanhPhan = new[]
            {
                Comp("Cơm trắng", 150, "g", riceId),
                Comp("Sườn heo nướng mật ong", 1, "miếng", ribId), // đơn vị chuẩn của thực phẩm: 120g
                Comp("Nước mắm pha", 20, "ml", null, new { caloTren100g = 60, chatDamTren100g = 2, tinhBotTren100g = 12, chatBeoTren100g = 0, chatXoTren100g = 0 }),
            },
        });
        Assert.True(confirm.Success, confirm.Code);
        var m = confirm.Data;
        Assert.True(m.GetProperty("daXacNhan").GetBoolean());
        // 150g cơm = 195 ; 120g sườn = 294 ; 20ml nước mắm = 12  → 501
        Assert.Equal(501m, m.GetProperty("tongDinhDuong").GetProperty("calo").GetDecimal());
        Assert.True(m.GetProperty("nguoiDungCoSua").GetBoolean());
        Assert.Equal(1, m.GetProperty("thanhPhan").EnumerateArray().Count(x => x.GetProperty("donViTinh").GetString() == "miếng"));
        Assert.Equal(Math.Round(501m / 2000m * 100m, 1), m.GetProperty("phanTramNhuCauNgay").GetDecimal()); // AC-39
        Assert.Equal(409, (await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Trua", tenMonAn = "X", thanhPhan = new[] { Comp("Cơm", 100, "g", riceId) } })).Status);

        var dash = await c.Get("/api/dashboard");
        var today = dash.Data.GetProperty("homNay");
        Assert.Equal(501m, today.GetProperty("calo").GetDecimal());
        Assert.Equal(1, today.GetProperty("soBuaAn").GetInt32());
        Assert.Equal(7, dash.Data.GetProperty("bayNgay").GetArrayLength());
        Assert.Equal(4, dash.Data.GetProperty("theoLoaiBuaAn").GetArrayLength());

        // Revert (bữa đã lưu): thành phần "Món lạ hoắc" không có dữ liệu → không thể hoàn tác
        Assert.Equal(ErrorCodes.NutritionDataMissing, (await c.Post($"/api/meals/{id}/revert")).Code);

        // AC-45: sửa khẩu phần → tính lại
        var upd = await c.Put($"/api/meals/{id}", new { loaiBuaAn = "Trua", tenMonAn = "Cơm tấm sườn nướng", thanhPhan = new[] { Comp("Cơm trắng", 100, "g", riceId) } });
        Assert.Equal(130m, upd.Data.GetProperty("tongDinhDuong").GetProperty("calo").GetDecimal());
        Assert.Equal(130m, (await c.Get("/api/dashboard")).Data.GetProperty("homNay").GetProperty("calo").GetDecimal());

        var hist = await c.Get("/api/meals");
        Assert.Equal(1, hist.Data.GetProperty("totalCount").GetInt32());
        Assert.Equal(130m, hist.Data.GetProperty("tongHopNgay")[0].GetProperty("tongCalo").GetDecimal());
        Assert.Equal(0, (await c.Get("/api/meals?loaiBuaAn=Sang")).Data.GetProperty("totalCount").GetInt32());

        // AC-44: người khác → 403
        var (other, _) = await NewVerifiedUser();
        Assert.Equal(403, (await other.Get($"/api/meals/{id}")).Status);
        Assert.Equal(403, (await other.Delete($"/api/meals/{id}")).Status);
        var admin = await LoginAdmin();
        Assert.Equal(403, (await admin.Delete($"/api/meals/{id}")).Status); // BR-34

        // AC-46: xóa mềm
        Assert.True((await c.Delete($"/api/meals/{id}")).Success);
        Assert.Equal(404, (await c.Get($"/api/meals/{id}")).Status);
        Assert.Equal(0, (await c.Get("/api/meals")).Data.GetProperty("totalCount").GetInt32());
        Assert.Equal(0m, (await c.Get("/api/dashboard")).Data.GetProperty("homNay").GetProperty("calo").GetDecimal());
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NutriVision.Infrastructure.Persistence.ApplicationDbContext>();
        Assert.True(db.BuaAns.IgnoreQueryFilters().Single(b => b.Id == id).DaXoa); // vẫn còn trong DB
    }

    [Fact] // AC-35: hoàn tác trên bản nháp & bữa không sửa → nguoiDungCoSua=false
    public async Task Revert_And_Unmodified_Confirmation()
    {
        Factory.Vision.Next = () => new VisionResult(true, new VisionDish("Phở bò", 0.88m, [new VisionIngredient("Phở bò tái nạm", 450)]), null, 500, "{}");
        var (c, _) = await NewVerifiedUser();
        var id = (await c.Scan(ApiClient.Png(), "pho.png", "Sang")).Data.GetProperty("id").GetGuid();
        var foodId = await FoodId(c, "phở bò");

        var reverted = await c.Post($"/api/meals/{id}/revert");
        Assert.Equal("Phở bò", reverted.Data.GetProperty("tenMonAn").GetString());

        var ok = await c.Post($"/api/meals/{id}/confirm", new { loaiBuaAn = "Sang", tenMonAn = "Phở bò", thanhPhan = new[] { Comp("Phở bò tái nạm", 450, "g", foodId) } });
        Assert.False(ok.Data.GetProperty("nguoiDungCoSua").GetBoolean());
        Assert.Equal(486m, ok.Data.GetProperty("tongDinhDuong").GetProperty("calo").GetDecimal());

        var edited = await c.Put($"/api/meals/{id}", new { loaiBuaAn = "Sang", tenMonAn = "Phở bò tái", thanhPhan = new[] { Comp("Phở bò tái nạm", 1, "tô", foodId) } });
        Assert.True(edited.Data.GetProperty("nguoiDungCoSua").GetBoolean());
        var back = await c.Post($"/api/meals/{id}/revert");
        Assert.Equal("Phở bò", back.Data.GetProperty("tenMonAn").GetString());
        Assert.False(back.Data.GetProperty("nguoiDungCoSua").GetBoolean());
    }

    [Fact] // AC-26, AC-27, AC-28, BR-24, BR-25
    public async Task Ai_Failure_And_Cancel_Deletes_Draft_And_Image()
    {
        var (c, _) = await NewVerifiedUser();
        Factory.Vision.Next = () => VisionResult.Fail("Dịch vụ AI không phản hồi trong 30 giây. Vui lòng nhập thủ công.", 90000);
        var failed = await c.Scan(ApiClient.Png(), "a.png");
        Assert.True(failed.Data.GetProperty("aiThatBai").GetBoolean());
        Assert.Contains("nhập thủ công", failed.Data.GetProperty("thongBaoAI").GetString());
        Assert.Equal(0, failed.Data.GetProperty("thanhPhan").GetArrayLength());
        var draftId = failed.Data.GetProperty("id").GetGuid();

        // Nhập tay trên bản nháp vẫn giữ ảnh
        var riceId = await FoodId(c, "cơm trắng");
        var saved = await c.Post($"/api/meals/{draftId}/confirm", new { loaiBuaAn = "Toi", tenMonAn = "Cơm", thanhPhan = new[] { Comp("Cơm", 1, "chén", riceId) } });
        Assert.True(saved.Success);
        Assert.False(saved.Data.GetProperty("taoTuAI").GetBoolean());
        Assert.NotNull(saved.Data.GetProperty("hinhAnhUrl").GetString());

        Factory.Vision.Next = () => new VisionResult(true, new VisionDish("", 0.1m, []), null, 100, "{}"); // rỗng
        var empty = await c.Scan(ApiClient.Png(), "b.png");
        Assert.True(empty.Data.GetProperty("aiThatBai").GetBoolean());

        Factory.Vision.Next = FakeVisionClient.Default;
        var draft = await c.Scan(ApiClient.Png(), "c.png");
        var did = draft.Data.GetProperty("id").GetGuid();
        var url = draft.Data.GetProperty("hinhAnhUrl").GetString()!;
        Assert.Equal(200, (await c.Send(HttpMethod.Get, url, auth: false)).Status);
        Assert.True((await c.Delete($"/api/meals/{did}/cancel")).Success);
        Assert.Equal(404, (await c.Get($"/api/meals/{did}")).Status);
        Assert.Equal(404, (await c.Send(HttpMethod.Get, url, auth: false)).Status); // ảnh đã bị xóa
        Assert.Equal(409, (await c.Delete($"/api/meals/{saved.Data.GetProperty("id").GetGuid()}/cancel")).Status);
    }

    [Fact] // AC-24, AC-25, BR-19
    public async Task Image_Validation()
    {
        var (c, _) = await NewVerifiedUser();
        var calls = Factory.Vision.Calls;
        var pdf = await c.Scan(Encoding.UTF8.GetBytes("%PDF-1.7 not an image"), "x.pdf", contentType: "application/pdf");
        Assert.Equal(ErrorCodes.InvalidImage, pdf.Code);
        Assert.Contains("Định dạng không hỗ trợ", pdf.Json.GetProperty("message").GetString());
        var small = await c.Scan(ApiClient.Png(100, 100), "s.png");
        Assert.Contains("224", small.Json.GetProperty("message").GetString());
        var huge = await c.Scan(new byte[10 * 1024 * 1024 + 100], "h.png");
        Assert.Equal(413, huge.Status);
        Assert.Equal(calls, Factory.Vision.Calls); // không gọi AI khi ảnh không hợp lệ
        Assert.Equal(400, (await c.Scan(ApiClient.Png(), "a.png", "Khuya")).Status);
    }

    [Fact] // AC-40..AC-42, AC-48, AC-50..AC-53, BR-38, BR-41
    public async Task Manual_Meals_Reports_Exports_Suggestions()
    {
        var (c, _) = await NewVerifiedUser();
        Assert.Equal(0, (await c.Get("/api/meals")).Data.GetProperty("totalCount").GetInt32()); // AC-42
        var dash = await c.Get("/api/dashboard");
        Assert.False(dash.Data.GetProperty("coDuLieu").GetBoolean()); // AC-48
        var emptyReport = await c.Get("/api/reports");
        Assert.Equal("info", emptyReport.Data.GetProperty("khuyenNghi")[0].GetProperty("muc").GetString());

        var pho = await FoodId(c, "phở bò");
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        for (var i = 0; i < 25; i++)
        {
            var r = await c.Post("/api/meals", new { loaiBuaAn = i % 2 == 0 ? "Sang" : "Toi", ngayAn = today.AddDays(-(i % 10)).ToString("yyyy-MM-dd"), tenMonAn = $"Phở {i}", thanhPhan = new[] { Comp("Phở", 1, "tô", pho) } });
            Assert.Equal(201, r.Status);
        }
        var future = await c.Post("/api/meals", new { loaiBuaAn = "Sang", ngayAn = today.AddDays(2).ToString("yyyy-MM-dd"), tenMonAn = "x", thanhPhan = new[] { Comp("Phở", 1, "tô", pho) } });
        Assert.Equal(400, future.Status);

        var page1 = await c.Get($"/api/meals?from={today.AddDays(-9):yyyy-MM-dd}&to={today:yyyy-MM-dd}");
        Assert.Equal(25, page1.Data.GetProperty("totalCount").GetInt32());
        Assert.Equal(20, page1.Data.GetProperty("items").GetArrayLength()); // AC-40
        Assert.Equal(2, page1.Data.GetProperty("totalPages").GetInt32());
        var dates = page1.Data.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("ngayAn").GetString()).ToList();
        Assert.Equal(dates.OrderByDescending(x => x).ToList(), dates);

        var range = await c.Get($"/api/meals?from={today.AddDays(-2):yyyy-MM-dd}&to={today:yyyy-MM-dd}"); // AC-41
        Assert.All(range.Data.GetProperty("items").EnumerateArray(), x => Assert.True(DateOnly.Parse(x.GetProperty("ngayAn").GetString()!) >= today.AddDays(-2)));
        Assert.Equal(400, (await c.Get($"/api/meals?from={today:yyyy-MM-dd}&to={today.AddDays(-1):yyyy-MM-dd}")).Status);

        var rep = await c.Get($"/api/reports?from={today.AddDays(-6):yyyy-MM-dd}&to={today:yyyy-MM-dd}");
        var rd = rep.Data;
        Assert.Equal(7, rd.GetProperty("soNgay").GetInt32());
        Assert.Equal(7, rd.GetProperty("theoNgay").GetArrayLength());
        Assert.Equal(7, rd.GetProperty("soNgayCoDuLieu").GetInt32());
        Assert.True(rd.GetProperty("trungBinhNgay").GetProperty("calo").GetDecimal() > 0);
        Assert.True(rd.GetProperty("khuyenNghi").GetArrayLength() > 0);
        Assert.Contains(rd.GetProperty("danhGia").GetProperty("calo").GetString(), new[] { "Thap", "DatChuan", "Cao" });

        var csv = await c.Get($"/api/reports/export?format=csv&from={today.AddDays(-6):yyyy-MM-dd}&to={today:yyyy-MM-dd}");
        Assert.Equal(200, csv.Status);
        var text = Encoding.UTF8.GetString(csv.Raw);
        Assert.Contains("Ngày,Calo", text);
        Assert.Contains("attachment", csv.Message.Content.Headers.ContentDisposition!.DispositionType);

        var pdf = await c.Get("/api/reports/export?format=pdf");
        Assert.Equal(200, pdf.Status);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf.Raw, 0, 4));

        var sug = await c.Get("/api/suggestions");
        Assert.True(sug.Data.GetArrayLength() is > 0 and <= 6);
        Assert.False(string.IsNullOrWhiteSpace(sug.Data[0].GetProperty("lyDo").GetString()));

        // BR-41: người khác xuất báo cáo không thấy dữ liệu này
        var (other, _) = await NewVerifiedUser();
        Assert.Equal(0, (await other.Get("/api/reports")).Data.GetProperty("tongSoBuaAn").GetInt32());

        var admin = await LoginAdmin();
        var stats = await admin.Get("/api/admin/statistics");
        Assert.True(stats.Data.GetProperty("tongBuaAn").GetInt32() >= 25);
        Assert.Equal(30, stats.Data.GetProperty("buaAnTheoNgay").GetArrayLength());
    }

    [Fact]
    public async Task Foods_Search_Is_Accent_Insensitive()
    {
        var c = await LoginUser();
        var r = await c.Get("/api/foods?q=pho bo");
        Assert.Contains(r.Data.EnumerateArray(), x => x.GetProperty("tenThucPham").GetString() == "Phở bò tái nạm");
        var m = await c.Get($"/api/foods/match?name={Uri.EscapeDataString("bún bò")}");
        Assert.True(m.Data.GetArrayLength() > 0);
        Assert.True((await c.Get("/api/foods/units")).Data.GetArrayLength() >= 10);
        Assert.True((await c.Get("/api/foods")).Data.GetArrayLength() > 0);
    }
}
