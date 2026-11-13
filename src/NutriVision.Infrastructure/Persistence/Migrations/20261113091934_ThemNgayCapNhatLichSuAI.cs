using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriVision.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemNgayCapNhatLichSuAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayCapNhat",
                table: "LichSuNhanDienAI",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            // Dữ liệu cũ: coi thời điểm cập nhật = thời điểm tạo.
            migrationBuilder.Sql("UPDATE [LichSuNhanDienAI] SET [NgayCapNhat] = [NgayTao]");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhanDienAI_NgayCapNhat",
                table: "LichSuNhanDienAI",
                column: "NgayCapNhat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LichSuNhanDienAI_NgayCapNhat",
                table: "LichSuNhanDienAI");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "LichSuNhanDienAI");
        }
    }
}
