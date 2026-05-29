using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("CanHo")]
public class CanHo
{
    [Key]
    public int ID { get; set; }

    [MaxLength(50)]
    public string? TenCanHo { get; set; }

    public int Tang {  get; set; }
    public int ID_NguoiDung {  get; set; }

    [ForeignKey(nameof(ID_NguoiDung))]
    public virtual NguoiDung? NguoiDung { get; set; }

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
    public virtual ICollection<YeuCauHoTro> YeuCauHoTros { get; set; } = new List<YeuCauHoTro>();
};