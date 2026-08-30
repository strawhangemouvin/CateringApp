using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("detail_pesanan")]
public class DetailPesanan
{
    [Key]
    [Column("detail_id")]
    public int DetailId { get; set; }

    [Column("pesanan_id")]
    public int PesananId { get; set; }

    [Column("paket_id")]
    public int PaketId { get; set; }

    [Column("jumlah")]
    public int Jumlah { get; set; }

    [Column("harga_satuan")]
    public decimal HargaSatuan { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("catatan")]
    public string? Catatan { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public virtual Pesanan? Pesanan { get; set; }
    public virtual PaketMenu? Paket { get; set; }
}