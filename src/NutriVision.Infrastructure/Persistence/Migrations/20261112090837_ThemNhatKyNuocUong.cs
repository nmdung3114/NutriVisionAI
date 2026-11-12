using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriVision.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemNhatKyNuocUong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyNuocUong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    LuongMl = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhat = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyNuocUong", x => x.Id);
                    table.CheckConstraint("CK_NhatKyNuocUong_LuongMl", "[LuongMl] >= 0 AND [LuongMl] <= 10000");
                    table.ForeignKey(
                        name: "FK_NhatKyNuocUong_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyNuocUong_NguoiDungId_Ngay",
                table: "NhatKyNuocUong",
                columns: new[] { "NguoiDungId", "Ngay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyNuocUong");
        }
    }
}
