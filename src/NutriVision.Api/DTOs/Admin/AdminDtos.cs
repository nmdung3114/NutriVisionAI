using System;
using System.Collections.Generic;

namespace NutriVision.Api.DTOs.Admin
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "USER";
        public string TrangThai { get; set; } = "ACTIVE";
        public bool EmailDaXacThuc { get; set; }
        public int SoBuaAnDaGhi { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }

    public class AdminStatsDto
    {
        public int TongNguoiDung { get; set; }
        public int TongNguoiDungActive { get; set; }
        public int TongBuaAnDaGhi { get; set; }
        public int TongLuotGoiAI { get; set; }
        public decimal TiLeAIChinhXac { get; set; } // % không cần người dùng sửa tên món
        public int ThoiGianPhanHoiTrungBinhMs { get; set; }
    }

    public class SystemLogDto
    {
        public Guid Id { get; set; }
        public string? NguoiThucHienEmail { get; set; }
        public string LoaiHanhDong { get; set; } = string.Empty;
        public string? BangTacDong { get; set; }
        public string? IdDoiTuong { get; set; }
        public string? ChiTiet { get; set; }
        public string? DiaChiIP { get; set; }
        public DateTimeOffset NgayTao { get; set; }
    }
}
