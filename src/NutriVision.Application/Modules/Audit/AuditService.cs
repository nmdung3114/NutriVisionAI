using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Domain.Entities;

namespace NutriVision.Application.Modules.Audit;

public interface IAuditService
{
    /// <summary>Thêm bản ghi nhật ký vào context (người gọi chịu trách nhiệm SaveChanges). BR-44.</summary>
    void Ghi(string loaiHanhDong, string? bangTacDong = null, string? idDoiTuong = null, string? chiTiet = null, Guid? nguoiThucHienId = null);
}

public class AuditService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock) : IAuditService
{
    public void Ghi(string loaiHanhDong, string? bangTacDong = null, string? idDoiTuong = null, string? chiTiet = null, Guid? nguoiThucHienId = null)
    {
        db.NhatKyHeThongs.Add(new NhatKyHeThong
        {
            NguoiDungThucHienId = nguoiThucHienId ?? currentUser.UserId,
            LoaiHanhDong = loaiHanhDong,
            BangTacDong = bangTacDong,
            IdDoiTuong = idDoiTuong,
            ChiTiet = chiTiet is { Length: > 2000 } ? chiTiet[..2000] : chiTiet,
            DiaChiIP = currentUser.IpAddress,
            NgayTao = clock.UtcNow,
        });
    }
}
