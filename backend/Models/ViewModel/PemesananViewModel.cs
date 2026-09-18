using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.ViewModel;

public class PemesananViewModel
{
    public int? PenggunaId { get; set; }

    [Display(Name = "Nama Pemesan (Walk-in / WA)")]
    public string? NamaPemesanManual { get; set; }

    [Display(Name = "No. Telepon / WhatsApp")]
    public string? TeleponPemesanManual { get; set; }

    [Required(ErrorMessage = "Pilih paket menu")]
    public int PaketId { get; set; }

    [Required(ErrorMessage = "Tentukan jumlah porsi")]
    [Range(10, 2000, ErrorMessage = "Minimal pemesanan paket katering adalah 10 porsi")]
    public int JumlahPorsi { get; set; } = 10;

    [Required(ErrorMessage = "Tentukan tanggal kirim")]
    [DataType(DataType.Date)]
    public DateTime TanggalPengiriman { get; set; } = DateTime.Today.AddDays(2);

    [Required(ErrorMessage = "Pilih slot jam pengantaran")]
    public string JamPengantaran { get; set; } = "10:00 - 12:00";

    [Required(ErrorMessage = "Alamat pengiriman wajib diisi")]
    public string AlamatPengiriman { get; set; } = string.Empty;

    public string? Catatan { get; set; }
}

public class UploadPembayaranViewModel
{
    public int PesananId { get; set; }
    public string NomorPesanan { get; set; } = string.Empty;
    public decimal TotalBayar { get; set; }

    [Required(ErrorMessage = "Pilih metode pembayaran")]
    public string MetodePembayaran { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unggah file bukti transfer")]
    public IFormFile? FileBukti { get; set; }
}
