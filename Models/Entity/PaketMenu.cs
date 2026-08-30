using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("paket_menu")]
public class PaketMenu
{
    [Key]
    [Column("paket_id")]
    public int PaketId { get; set; }

    [Column("kategori_id")]
    public int KategoriId { get; set; }

    [Column("nama_paket")]
    public string NamaPaket { get; set; } = string.Empty;

    [Column("harga")]
    public decimal Harga { get; set; }

    [Column("deskripsi_menu")]
    public string? DeskripsiMenu { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("gambar")]
    public string? Gambar { get; set; }

    public virtual KategoriMenu? Kategori { get; set; }
}