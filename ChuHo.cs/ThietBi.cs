using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("ThietBi")]
public class ThietBi
{
    [Key]
    public int ID { get; set; }

    [MaxLength(100)]
    public string? TenThietBi { get; set; } 
    // Ví dụ: "Tivi Phòng Khách"

    [MaxLength(50)]
    public string? LoaiThietBi { get; set; } 
    // Ví dụ: "Tivi", "DieuHoa", "CuaChinh"

    // Lưu trạng thái Bật/Tắt, Đóng/Mở
    public bool TrangThai { get; set; }

    public int ID_Phong { get; set; }

    [ForeignKey("ID_Phong")]
    public Phong? Phong { get; set; }
}