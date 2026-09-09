using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("pesanan")]
public class Pesanan
{
    [Key]
    [Column("pesanan_id")]
    public int PesananId { get; set; }

    [Column("pengguna_id")]
    public int PenggunaId { get; set; }

    [Column("nomor_pesanan")]
    public string NomorPesanan { get; set; } = string.Empty;

    [Column("tanggal_pesan")]
    public DateTime TanggalPesan { get; set; }

    [Column("tanggal_pengiriman")]
    public DateTime TanggalPengiriman { get; set; }

    [Column("alamat_pengiriman")]
    public string AlamatPengiriman { get; set; } = string.Empty;

    [Column("total_bayar")]
    public decimal TotalBayar { get; set; }

    [Column("status_pesanan")]
    public string StatusPesanan { get; set; } = "Pending";

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("refund_nama_bank")]
    public string? RefundNamaBank { get; set; }

    [Column("refund_no_rekening")]
    public string? RefundNoRekening { get; set; }

    [Column("refund_atas_nama")]
    public string? RefundAtasNama { get; set; }

    [Column("refund_bukti_tf")]
    public string? RefundBuktiTf { get; set; }

    public virtual Pengguna? Pengguna { get; set; }
    public virtual ICollection<DetailPesanan> DetailPesanans { get; set; } = new List<DetailPesanan>();
    public virtual Pembayaran? Pembayaran { get; set; }
}
