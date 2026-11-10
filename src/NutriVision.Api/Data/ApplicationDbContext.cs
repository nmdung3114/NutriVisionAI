using System;
using Microsoft.EntityFrameworkCore;
using NutriVision.Api.Models.Entities;

namespace NutriVision.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
        public DbSet<HoSoNguoiDung> HoSoNguoiDungs => Set<HoSoNguoiDung>();
        public DbSet<TokenLamMoi> TokenLamMois => Set<TokenLamMoi>();
        public DbSet<ThucPhamThamChieu> ThucPhamThamChieus => Set<ThucPhamThamChieu>();
        public DbSet<BuaAn> BuaAns => Set<BuaAn>();
        public DbSet<ThanhPhanBuaAn> ThanhPhanBuaAns => Set<ThanhPhanBuaAn>();
        public DbSet<TongHopDinhDuongNgay> TongHopDinhDuongNgays => Set<TongHopDinhDuongNgay>();
        public DbSet<LichSuNhanDienAI> LichSuNhanDienAIs => Set<LichSuNhanDienAI>();
        public DbSet<NhatKyHeThong> NhatKyHeThongs => Set<NhatKyHeThong>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. NguoiDung Configuration
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.ToTable("NguoiDung");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique(); // BR-02: Email duy nhất
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.HoTen).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TrangThai).HasMaxLength(30).IsRequired();
                entity.Property(e => e.VaiTro).HasMaxLength(30).IsRequired();

                // 1 - 1 với HoSoNguoiDung
                entity.HasOne(e => e.HoSo)
                      .WithOne(h => h.NguoiDung)
                      .HasForeignKey<HoSoNguoiDung>(h => h.NguoiDungId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 2. HoSoNguoiDung Configuration
            modelBuilder.Entity<HoSoNguoiDung>(entity =>
            {
                entity.ToTable("HoSoNguoiDung");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ChieuCao).HasPrecision(5, 2);
                entity.Property(e => e.CanNang).HasPrecision(5, 2);
                entity.Property(e => e.ChiSoBMI).HasPrecision(4, 2);
                entity.Property(e => e.ChiSoBMR).HasPrecision(6, 2);
                entity.Property(e => e.ChiSoTDEE).HasPrecision(6, 2);
                entity.Property(e => e.MucTieuCalo).HasPrecision(6, 2);
                entity.Property(e => e.GioiTinh).HasMaxLength(10);
                entity.Property(e => e.MucDoVanDong).HasMaxLength(30);
                entity.Property(e => e.DuongDanAnhDaiDien).HasMaxLength(500);
            });

            // 3. TokenLamMoi Configuration
            modelBuilder.Entity<TokenLamMoi>(entity =>
            {
                entity.ToTable("TokenLamMoi");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaToken).HasMaxLength(500).IsRequired();
                
                entity.HasOne(e => e.NguoiDung)
                      .WithMany(u => u.Tokens)
                      .HasForeignKey(e => e.NguoiDungId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. ThucPhamThamChieu Configuration
            modelBuilder.Entity<ThucPhamThamChieu>(entity =>
            {
                entity.ToTable("ThucPhamThamChieu");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.MaThucPham).IsUnique();
                entity.Property(e => e.MaThucPham).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TenThucPham).HasMaxLength(200).IsRequired();
                entity.Property(e => e.NhomThucPham).HasMaxLength(100);
                entity.Property(e => e.DonViChuan).HasMaxLength(50);
                entity.Property(e => e.KhoiLuongChuan).HasPrecision(6, 2);

                entity.Property(e => e.CaloTren100g).HasPrecision(6, 2);
                entity.Property(e => e.ChatDamTren100g).HasPrecision(6, 2);
                entity.Property(e => e.TinhBotTren100g).HasPrecision(6, 2);
                entity.Property(e => e.ChatBeoTren100g).HasPrecision(6, 2);
                entity.Property(e => e.ChatXoTren100g).HasPrecision(6, 2);
            });

            // 5. BuaAn Configuration
            modelBuilder.Entity<BuaAn>(entity =>
            {
                entity.ToTable("BuaAn");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LoaiBuaAn).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TenMonAn).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DuongDanHinhAnh).HasMaxLength(500);

                entity.Property(e => e.TongCalo).HasPrecision(7, 2);
                entity.Property(e => e.TongChatDam).HasPrecision(6, 2);
                entity.Property(e => e.TongTinhBot).HasPrecision(6, 2);
                entity.Property(e => e.TongChatBeo).HasPrecision(6, 2);
                entity.Property(e => e.TongChatXo).HasPrecision(6, 2);

                // Global Query Filter: Tự động lọc các bữa ăn chưa bị xóa mềm (BR-35)
                entity.HasQueryFilter(b => !b.DaXoa);

                entity.HasOne(e => e.NguoiDung)
                      .WithMany(u => u.BuaAns)
                      .HasForeignKey(e => e.NguoiDungId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. ThanhPhanBuaAn Configuration
            modelBuilder.Entity<ThanhPhanBuaAn>(entity =>
            {
                entity.ToTable("ThanhPhanBuaAn");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenThanhPhan).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DonViTinh).HasMaxLength(50).IsRequired();
                entity.Property(e => e.KhauPhan).HasPrecision(6, 2);
                entity.Property(e => e.KhoiLuongQuyDoiGram).HasPrecision(6, 2);

                entity.Property(e => e.LuongCalo).HasPrecision(7, 2);
                entity.Property(e => e.LuongChatDam).HasPrecision(6, 2);
                entity.Property(e => e.LuongTinhBot).HasPrecision(6, 2);
                entity.Property(e => e.LuongChatBeo).HasPrecision(6, 2);
                entity.Property(e => e.LuongChatXo).HasPrecision(6, 2);

                entity.HasOne(e => e.BuaAn)
                      .WithMany(b => b.ThanhPhans)
                      .HasForeignKey(e => e.BuaAnId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ThucPhamThamChieu)
                      .WithMany(t => t.ThanhPhans)
                      .HasForeignKey(e => e.ThucPhamThamChieuId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // 7. TongHopDinhDuongNgay Configuration
            modelBuilder.Entity<TongHopDinhDuongNgay>(entity =>
            {
                entity.ToTable("TongHopDinhDuongNgay");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.NguoiDungId, e.Ngay }).IsUnique();

                entity.Property(e => e.TongCalo).HasPrecision(7, 2);
                entity.Property(e => e.TongChatDam).HasPrecision(6, 2);
                entity.Property(e => e.TongTinhBot).HasPrecision(6, 2);
                entity.Property(e => e.TongChatBeo).HasPrecision(6, 2);
                entity.Property(e => e.TongChatXo).HasPrecision(6, 2);
                entity.Property(e => e.MucTieuCaloNgay).HasPrecision(6, 2);

                entity.HasOne(e => e.NguoiDung)
                      .WithMany(u => u.TongHopDinhDuongs)
                      .HasForeignKey(e => e.NguoiDungId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. LichSuNhanDienAI Configuration
            modelBuilder.Entity<LichSuNhanDienAI>(entity =>
            {
                entity.ToTable("LichSuNhanDienAI");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DuongDanHinhAnh).HasMaxLength(500).IsRequired();
                entity.Property(e => e.TenMonAIDoan).HasMaxLength(200);
                entity.Property(e => e.TenMonNguoiDungChot).HasMaxLength(200);
                entity.Property(e => e.DoTinCay).HasPrecision(4, 3);

                entity.HasOne(e => e.NguoiDung)
                      .WithMany(u => u.LichSuNhanDiens)
                      .HasForeignKey(e => e.NguoiDungId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BuaAn)
                      .WithMany(b => b.LichSuNhanDiens)
                      .HasForeignKey(e => e.BuaAnId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // 9. NhatKyHeThong Configuration
            modelBuilder.Entity<NhatKyHeThong>(entity =>
            {
                entity.ToTable("NhatKyHeThong");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LoaiHanhDong).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BangTacDong).HasMaxLength(50);
                entity.Property(e => e.IdDoiTuong).HasMaxLength(100);
                entity.Property(e => e.DiaChiIP).HasMaxLength(50);

                entity.HasOne(e => e.NguoiDungThucHien)
                      .WithMany(u => u.NhatKyHeThongs)
                      .HasForeignKey(e => e.NguoiDungThucHienId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
