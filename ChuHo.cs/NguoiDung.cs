using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("NguoiDung")]
public class NguoiDung
{
    [Key]
    public int ID { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? SDT { get; set; }

    [MaxLength(20)]
    public string? Account { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<CanHo> CanHos { get; set; } = new List<CanHo>();
};