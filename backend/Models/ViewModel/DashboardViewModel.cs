using CateringApp.Models.Entity;
using System.Collections.Generic;

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
    public List<Pesanan> PesananButuhRefund { get; set; } = new();
    public List<DailyOmsetViewModel> DailyOmset { get; set; } = new();
}

public class DailyOmsetViewModel
{
    public string Tanggal { get; set; } = string.Empty;
    public decimal Omset { get; set; }
    public int JumlahPesanan { get; set; }
}

public class CustomerDashboardViewModel
{
    public int TotalPesanan { get; set; }
    public decimal TotalBelanja { get; set; }
    public int PesananAktif { get; set; }
    public List<Pesanan> PesananTerbaru { get; set; } = new();
    public List<Pesanan> PesananButuhRefund { get; set; } = new();
}

public class NotifikasiItemViewModel
{
    public string Judul { get; set; } = string.Empty;
    public string Pesan { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Waktu { get; set; } = string.Empty;
    public string Tipe { get; set; } = "info"; // danger, warning, success, info
    public string Icon { get; set; } = "fa-bell";
}
