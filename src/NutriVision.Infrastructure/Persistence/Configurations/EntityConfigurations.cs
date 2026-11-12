using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriVision.Domain.Entities;

namespace NutriVision.Infrastructure.Persistence.Configurations;

public class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> b)
    {
        b.ToTable("NguoiDung");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.MatKhauHash).IsRequired();
        b.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
        b.Property(x => x.TrangThai).HasMaxLength(30).IsRequired();
        b.Property(x => x.VaiTro).HasMaxLength(30).IsRequired();
        b.Property(x => x.TokenXacThucEmail).HasMaxLength(128);
        b.Property(x => x.TokenDatLaiMatKhau).HasMaxLength(128);
        b.HasIndex(x => x.TokenXacThucEmail);
        b.HasIndex(x => x.TokenDatLaiMatKhau);
        b.HasOne(x => x.HoSo).WithOne(h => h.NguoiDung).HasForeignKey<HoSoNguoiDung>(h => h.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.TokenLamMois).WithOne(t => t.NguoiDung).HasForeignKey(t => t.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class HoSoNguoiDungConfiguration : IEntityTypeConfiguration<HoSoNguoiDung>
{
    public void Configure(EntityTypeBuilder<HoSoNguoiDung> b)
    {
        b.ToTable("HoSoNguoiDung");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.NguoiDungId).IsUnique();
        b.Property(x => x.GioiTinh).HasMaxLength(10).IsRequired();
        b.Property(x => x.ChieuCao).HasPrecision(5, 2);
        b.Property(x => x.CanNang).HasPrecision(5, 2);
        b.Property(x => x.MucDoVanDong).HasMaxLength(30).IsRequired();
        b.Property(x => x.ChiSoBMI).HasPrecision(4, 2);
        b.Property(x => x.ChiSoBMR).HasPrecision(6, 2);
        b.Property(x => x.ChiSoTDEE).HasPrecision(6, 2);
        b.Property(x => x.MucTieuCalo).HasPrecision(6, 2);
        b.Property(x => x.DuongDanAnhDaiDien).HasMaxLength(500);
    }
}

public class TokenLamMoiConfiguration : IEntityTypeConfiguration<TokenLamMoi>
{
    public void Configure(EntityTypeBuilder<TokenLamMoi> b)
    {
        b.ToTable("TokenLamMoi");
        b.HasKey(x => x.Id);
        b.Property(x => x.MaToken).HasMaxLength(500).IsRequired();
        b.HasIndex(x => x.MaToken).IsUnique();
        b.HasIndex(x => x.NguoiDungId);
    }
}

public class ThucPhamThamChieuConfiguration : IEntityTypeConfiguration<ThucPhamThamChieu>
{
    public void Configure(EntityTypeBuilder<ThucPhamThamChieu> b)
    {
        b.ToTable("ThucPhamThamChieu");
        b.HasKey(x => x.Id);
        b.Property(x => x.MaThucPham).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaThucPham).IsUnique();
        b.Property(x => x.TenThucPham).HasMaxLength(200).IsRequired();
        b.Property(x => x.NhomThucPham).HasMaxLength(100).IsRequired();
        b.Property(x => x.DonViChuan).HasMaxLength(50).IsRequired();
        b.Property(x => x.KhoiLuongChuan).HasPrecision(6, 2);
        b.Property(x => x.CaloTren100g).HasPrecision(6, 2);
        b.Property(x => x.ChatDamTren100g).HasPrecision(6, 2);
        b.Property(x => x.TinhBotTren100g).HasPrecision(6, 2);
        b.Property(x => x.ChatBeoTren100g).HasPrecision(6, 2);
        b.Property(x => x.ChatXoTren100g).HasPrecision(6, 2);
        b.Ignore(x => x.DinhDuongTren100g);
    }
}

public class BuaAnConfiguration : IEntityTypeConfiguration<BuaAn>
{
    public void Configure(EntityTypeBuilder<BuaAn> b)
    {
        b.ToTable("BuaAn");
        b.HasKey(x => x.Id);
        b.Property(x => x.LoaiBuaAn).HasMaxLength(20).IsRequired();
        b.Property(x => x.TenMonAn).HasMaxLength(200).IsRequired();
        b.Property(x => x.DuongDanHinhAnh).HasMaxLength(500);
        b.Property(x => x.TongCalo).HasPrecision(7, 2);
        b.Property(x => x.TongChatDam).HasPrecision(6, 2);
        b.Property(x => x.TongTinhBot).HasPrecision(6, 2);
        b.Property(x => x.TongChatBeo).HasPrecision(6, 2);
        b.Property(x => x.TongChatXo).HasPrecision(6, 2);
        b.Ignore(x => x.TongDinhDuong);
        b.Ignore(x => x.TinhVaoTongHop);
        b.HasIndex(x => new { x.NguoiDungId, x.NgayAn });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.ThanhPhans).WithOne(t => t.BuaAn).HasForeignKey(t => t.BuaAnId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.LichSuNhanDien).WithOne(l => l.BuaAn).HasForeignKey<LichSuNhanDienAI>(l => l.BuaAnId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ThanhPhanBuaAnConfiguration : IEntityTypeConfiguration<ThanhPhanBuaAn>
{
    public void Configure(EntityTypeBuilder<ThanhPhanBuaAn> b)
    {
        b.ToTable("ThanhPhanBuaAn");
        b.HasKey(x => x.Id);
        b.Property(x => x.TenThanhPhan).HasMaxLength(200).IsRequired();
        b.Property(x => x.KhauPhan).HasPrecision(6, 2);
        b.Property(x => x.DonViTinh).HasMaxLength(50).IsRequired();
        b.Property(x => x.KhoiLuongQuyDoiGram).HasPrecision(6, 2);
        b.Property(x => x.LuongCalo).HasPrecision(7, 2);
        b.Property(x => x.LuongChatDam).HasPrecision(6, 2);
        b.Property(x => x.LuongTinhBot).HasPrecision(6, 2);
        b.Property(x => x.LuongChatBeo).HasPrecision(6, 2);
        b.Property(x => x.LuongChatXo).HasPrecision(6, 2);
        b.Ignore(x => x.DinhDuong);
        b.HasIndex(x => x.BuaAnId);
        b.HasOne(x => x.ThucPhamThamChieu).WithMany().HasForeignKey(x => x.ThucPhamThamChieuId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class TongHopDinhDuongNgayConfiguration : IEntityTypeConfiguration<TongHopDinhDuongNgay>
{
    public void Configure(EntityTypeBuilder<TongHopDinhDuongNgay> b)
    {
        b.ToTable("TongHopDinhDuongNgay");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.NguoiDungId, x.Ngay }).IsUnique();
        b.Property(x => x.TongCalo).HasPrecision(7, 2);
        b.Property(x => x.TongChatDam).HasPrecision(6, 2);
        b.Property(x => x.TongTinhBot).HasPrecision(6, 2);
        b.Property(x => x.TongChatBeo).HasPrecision(6, 2);
        b.Property(x => x.TongChatXo).HasPrecision(6, 2);
        b.Property(x => x.MucTieuCaloNgay).HasPrecision(6, 2);
        b.Ignore(x => x.Tong);
        b.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LichSuNhanDienAIConfiguration : IEntityTypeConfiguration<LichSuNhanDienAI>
{
    public void Configure(EntityTypeBuilder<LichSuNhanDienAI> b)
    {
        b.ToTable("LichSuNhanDienAI");
        b.HasKey(x => x.Id);
        b.Property(x => x.DuongDanHinhAnh).HasMaxLength(500).IsRequired();
        b.Property(x => x.TenMonAIDoan).HasMaxLength(200);
        b.Property(x => x.TenMonNguoiDungChot).HasMaxLength(200);
        b.Property(x => x.DoTinCay).HasPrecision(4, 3);
        b.HasIndex(x => x.NguoiDungId);
        b.HasIndex(x => x.BuaAnId).IsUnique();
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class NhatKyHeThongConfiguration : IEntityTypeConfiguration<NhatKyHeThong>
{
    public void Configure(EntityTypeBuilder<NhatKyHeThong> b)
    {
        b.ToTable("NhatKyHeThong");
        b.HasKey(x => x.Id);
        b.Property(x => x.LoaiHanhDong).HasMaxLength(50).IsRequired();
        b.Property(x => x.BangTacDong).HasMaxLength(50);
        b.Property(x => x.IdDoiTuong).HasMaxLength(100);
        b.Property(x => x.DiaChiIP).HasMaxLength(50);
        b.HasIndex(x => x.NgayTao);
        b.HasIndex(x => x.NguoiDungThucHienId);
        b.HasOne(x => x.NguoiDungThucHien).WithMany().HasForeignKey(x => x.NguoiDungThucHienId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class NhatKyCanNangConfiguration : IEntityTypeConfiguration<NhatKyCanNang>
{
    public void Configure(EntityTypeBuilder<NhatKyCanNang> b)
    {
        b.ToTable("NhatKyCanNang");
        b.HasKey(x => x.Id);
        b.Property(x => x.CanNang).HasPrecision(5, 2);
        b.Property(x => x.GhiChu).HasMaxLength(200);
        b.HasIndex(x => new { x.NguoiDungId, x.Ngay }).IsUnique();
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NhatKyNuocUongConfiguration : IEntityTypeConfiguration<NhatKyNuocUong>
{
    public void Configure(EntityTypeBuilder<NhatKyNuocUong> b)
    {
        b.ToTable("NhatKyNuocUong", t => t.HasCheckConstraint("CK_NhatKyNuocUong_LuongMl", "[LuongMl] >= 0 AND [LuongMl] <= 10000"));
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.NguoiDungId, x.Ngay }).IsUnique();
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.Cascade);
    }
}
