using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriVision.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemNhatKyCanNang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyCanNang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    CanNang = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyCanNang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyCanNang_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyCanNang_NguoiDungId_Ngay",
                table: "NhatKyCanNang",
                columns: new[] { "NguoiDungId", "Ngay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyCanNang");
        }
    }
}
