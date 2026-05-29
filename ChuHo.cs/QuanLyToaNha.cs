using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("QuanLyToaNha")]
public class QuanLyToaNha
{
    [Key]
    public int ID { get; set; }

    [MaxLength(50)]
    public required string TaiKhoan { get; set; }

    public required string MatKhau { get; set; }

    [MaxLength(100)]
    public required string HoTen { get; set; }

    [MaxLength(50)]
    public string? VaiTro { get; set; } // Admin, Supervisor...
}

[Table("NhanVien")]
public class NhanVien
{
    [Key]
    public int ID { get; set; }

    [MaxLength(100)]
    public required string HoTen { get; set; }

    [MaxLength(50)]
    public string? ChucVu { get; set; }

    [MaxLength(20)]
    public string? SDT { get; set; }
}

[Table("YeuCauHoTro")]
public class YeuCauHoTro
{
    [Key]
    public int ID { get; set; }

    public int ID_CanHo { get; set; }

    [ForeignKey(nameof(ID_CanHo))]
    public virtual CanHo? CanHo { get; set; }

    public required string NoiDung { get; set; }

    public DateTime ThoiGian { get; set; }

    [MaxLength(50)]
    public required string TrangThai { get; set; } // Chờ xử lý / Đang xử lý / Đã xong
}