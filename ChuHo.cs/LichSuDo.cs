using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBuilding.Domain.Entities;

[Table("LichSuDo")]
public class LichSuDo
{
    [Key]
    public long ID { get; set; } // Dùng kiểu long (bigint) vì dữ liệu sinh ra liên tục

    public int ID_CamBien { get; set; }

    [ForeignKey(nameof(ID_CamBien))]
    public virtual CamBien? CamBien { get; set; }

    public double GiaTri { get; set; }

    public DateTime ThoiGian { get; set; }
}