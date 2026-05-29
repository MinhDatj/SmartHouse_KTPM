using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("Phong")]
public class Phong
{
    [Key]
    public int ID { get; set; }

    [MaxLength(50)]
    public string TenPhong { get; set; }

    public int ID_CanHo {  get; set; }

    [ForeignKey(nameof(ID_CanHo))]
    public virtual CanHo? CanHo { get; set; }

    public virtual ICollection<CamBien> Cambiens { get; set; } = new List<CamBien>();
}