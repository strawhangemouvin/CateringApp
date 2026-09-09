using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("peran")]
public class Peran
{
    [Key]
    [Column("peran_id")]
    public int PeranId { get; set; }

    [Column("nama_peran")]
    public string NamaPeran { get; set; } = string.Empty;
}
