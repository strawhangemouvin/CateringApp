using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("kategori_menu")]
public class KategoriMenu
{
    [Key]
    [Column("kategori_id")]
    public int KategoriId { get; set; }

    [Column("nama_kategori")]
    public string NamaKategori { get; set; } = string.Empty;

    [Column("deskripsi")]
    public string? Deskripsi { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
