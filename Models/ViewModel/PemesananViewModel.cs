using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.ViewModel;

public class PemesananViewModel
{
    [Required(ErrorMessage = "Pilih paket menu")]
    public int PaketId { get; set; }

    [Required(ErrorMessage = "Tentukan jumlah porsi")]
    [Range(1, 1000, ErrorMessage = "Minimal pemesanan 1 porsi")]
    public int JumlahPorsi { get; set; }

    [Required(ErrorMessage = "Tentukan tanggal kirim")]
    [DataType(DataType.Date)]
    public DateTime TanggalPengiriman { get; set; } = DateTime.Today.AddDays(2);

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
