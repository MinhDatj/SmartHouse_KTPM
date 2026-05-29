using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("Cambien")]
public class CamBien
{
    [Key]
    public int ID { get; set; }

    [MaxLength(100)]
    public string? TenCamBien {  get; set; }

    [MaxLength(50)]
    public string? LoaiCamBien { get; set; }

    [MaxLength(50)]
    public string? TrangThai {  get; set; }

    public int ID_phong {  get; set; }

    [ForeignKey(nameof(ID_phong))]
    public virtual Phong? Phong {  get; set; }

    public virtual ICollection<LichSuDo> LichSuDos { get; set; } = new List<LichSuDo>(); 
}