using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("pembayaran")]
public class Pembayaran
{
    [Key]
    [Column("pembayaran_id")]
    public int PembayaranId { get; set; }

    [Column("pesanan_id")]
    public int PesananId { get; set; }

    [Column("metode_pembayaran")]
    public string MetodePembayaran { get; set; } = string.Empty;

    [Column("jumlah_bayar")]
    public decimal JumlahBayar { get; set; }

    [Column("tanggal_bayar")]
    public DateTime TanggalBayar { get; set; }

    [Column("bukti_transfer")]
    public string? BuktiTransfer { get; set; }

    [Column("status_verifikasi")]
    public string StatusVerifikasi { get; set; } = "Menunggu Verifikasi";

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public virtual Pesanan? Pesanan { get; set; }
}