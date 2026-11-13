using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriVision.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailDaXacThuc = table.Column<bool>(type: "bit", nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SoLanDangNhapSai = table.Column<int>(type: "int", nullable: false),
                    ThoiGianHetHanKhoa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TokenXacThucEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ThoiGianHetHanXacThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TokenDatLaiMatKhau = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ThoiGianHetHanDatLai = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThucPhamThamChieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaThucPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenThucPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NhomThucPham = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonViChuan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KhoiLuongChuan = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    CaloTren100g = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ChatDamTren100g = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TinhBotTren100g = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ChatBeoTren100g = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ChatXoTren100g = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThucPhamThamChieu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuaAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiBuaAn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayAn = table.Column<DateOnly>(type: "date", nullable: false),
                    ThoiGianGhiNhan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenMonAn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DuongDanHinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TongCalo = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    TongChatDam = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongTinhBot = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongChatBeo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongChatXo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TaoTuAI = table.Column<bool>(type: "bit", nullable: false),
                    DaXacNhan = table.Column<bool>(type: "bit", nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuaAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuaAn_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoSoNguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChieuCao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CanNang = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MucDoVanDong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChiSoBMI = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    ChiSoBMR = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ChiSoTDEE = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    MucTieuCalo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    DuongDanAnhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoNguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoNguoiDung_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungThucHienId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoaiHanhDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BangTacDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdDoiTuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyHeThong_NguoiDung_NguoiDungThucHienId",
                        column: x => x.NguoiDungThucHienId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TokenLamMoi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThoiGianHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false),
                    DaThuHoi = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenLamMoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenLamMoi_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TongHopDinhDuongNgay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    TongCalo = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    TongChatDam = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongTinhBot = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongChatBeo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TongChatXo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    SoBuaAnDaGhi = table.Column<int>(type: "int", nullable: false),
                    MucTieuCaloNgay = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TongHopDinhDuongNgay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TongHopDinhDuongNgay_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuNhanDienAI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuaAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuongDanHinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhanHoiAIGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenMonAIDoan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenMonNguoiDungChot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoTinCay = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: true),
                    NguoiDungCoSua = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGianXuLyMs = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuNhanDienAI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuNhanDienAI_BuaAn_BuaAnId",
                        column: x => x.BuaAnId,
                        principalTable: "BuaAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LichSuNhanDienAI_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThanhPhanBuaAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuaAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThucPhamThamChieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenThanhPhan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KhauPhan = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KhoiLuongQuyDoiGram = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    LuongCalo = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    LuongChatDam = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    LuongTinhBot = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    LuongChatBeo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    LuongChatXo = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhPhanBuaAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhPhanBuaAn_BuaAn_BuaAnId",
                        column: x => x.BuaAnId,
                        principalTable: "BuaAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThanhPhanBuaAn_ThucPhamThamChieu_ThucPhamThamChieuId",
                        column: x => x.ThucPhamThamChieuId,
                        principalTable: "ThucPhamThamChieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuaAn_NguoiDungId_NgayAn",
                table: "BuaAn",
                columns: new[] { "NguoiDungId", "NgayAn" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoNguoiDung_NguoiDungId",
                table: "HoSoNguoiDung",
                column: "NguoiDungId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhanDienAI_BuaAnId",
                table: "LichSuNhanDienAI",
                column: "BuaAnId",
                unique: true,
                filter: "[BuaAnId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhanDienAI_NguoiDungId",
                table: "LichSuNhanDienAI",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TokenDatLaiMatKhau",
                table: "NguoiDung",
                column: "TokenDatLaiMatKhau");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TokenXacThucEmail",
                table: "NguoiDung",
                column: "TokenXacThucEmail");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThong_NgayTao",
                table: "NhatKyHeThong",
                column: "NgayTao");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThong_NguoiDungThucHienId",
                table: "NhatKyHeThong",
                column: "NguoiDungThucHienId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanBuaAn_BuaAnId",
                table: "ThanhPhanBuaAn",
                column: "BuaAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanBuaAn_ThucPhamThamChieuId",
                table: "ThanhPhanBuaAn",
                column: "ThucPhamThamChieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ThucPhamThamChieu_MaThucPham",
                table: "ThucPhamThamChieu",
                column: "MaThucPham",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenLamMoi_MaToken",
                table: "TokenLamMoi",
                column: "MaToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenLamMoi_NguoiDungId",
                table: "TokenLamMoi",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_TongHopDinhDuongNgay_NguoiDungId_Ngay",
                table: "TongHopDinhDuongNgay",
                columns: new[] { "NguoiDungId", "Ngay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoSoNguoiDung");

            migrationBuilder.DropTable(
                name: "LichSuNhanDienAI");

            migrationBuilder.DropTable(
                name: "NhatKyHeThong");

            migrationBuilder.DropTable(
                name: "ThanhPhanBuaAn");

            migrationBuilder.DropTable(
                name: "TokenLamMoi");

            migrationBuilder.DropTable(
                name: "TongHopDinhDuongNgay");

            migrationBuilder.DropTable(
                name: "BuaAn");

            migrationBuilder.DropTable(
                name: "ThucPhamThamChieu");

            migrationBuilder.DropTable(
                name: "NguoiDung");
        }
    }
}
