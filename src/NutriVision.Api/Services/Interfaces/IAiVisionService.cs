using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NutriVision.Api.Services.Interfaces
{
    public class AiVisionResult
    {
        public bool IsSuccess { get; set; }
        public string TenMonAn { get; set; } = string.Empty;
        public decimal DoTinCay { get; set; } = 0.92m;
        public List<AiFoodComponentResult> ThanhPhans { get; set; } = new();
        public string RawJsonResponse { get; set; } = string.Empty;
        public int ThoiGianXuLyMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AiFoodComponentResult
    {
        public string TenThanhPhan { get; set; } = string.Empty;
        public decimal KhauPhan { get; set; } = 1;
        public string DonViTinh { get; set; } = "phần";
        public decimal KhoiLuongQuyDoiGram { get; set; } = 100;
    }

    public interface IAiVisionService
    {
        Task<AiVisionResult> AnalyzeMealImageAsync(string imagePath, Stream imageStream, string originalFileName);
    }
}
