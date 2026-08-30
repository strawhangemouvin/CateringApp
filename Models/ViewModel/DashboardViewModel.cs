using CateringApp.Models.Entity;

namespace CateringApp.Models.ViewModel;

public class DashboardViewModel
{
    public int TotalPesanan { get; set; }
    public decimal TotalPendapatan { get; set; }
    public int PesananPending { get; set; }
    public int PesananDiproses { get; set; }
    public int TotalPelanggan { get; set; }
    public List<Pesanan> PesananTerbaru { get; set; } = new();
    public List<Pesanan> JadwalPengiriman { get; set; } = new();
    public List<Pesanan> PesananButuhVerifikasi { get; set; } = new();
    public List<DailyOmsetViewModel> DailyOmset { get; set; } = new();
}

public class DailyOmsetViewModel
{
    public string Tanggal { get; set; } = string.Empty;
    public decimal Omset { get; set; }
    public int JumlahPesanan { get; set; }
}