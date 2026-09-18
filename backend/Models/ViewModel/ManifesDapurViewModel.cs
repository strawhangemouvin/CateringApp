using System;
using System.Collections.Generic;

namespace CateringApp.Models.ViewModel
{
    public class ManifesDapurViewModel
    {
        public DateTime Tanggal { get; set; } = DateTime.Today;
        public int TotalPorsiKolektif { get; set; }
        public int TotalPesanan { get; set; }
        public int KapasitasMaksimalDapur { get; set; } = 400;
        public int SisaKapasitas => Math.Max(0, KapasitasMaksimalDapur - TotalPorsiKolektif);
        public List<ManifesItemViewModel> Items { get; set; } = new();
        public List<ManifesPesananRingkasViewModel> DaftarPesananHariIni { get; set; } = new();
    }

    public class ManifesItemViewModel
    {
        public int PaketId { get; set; }
        public string NamaPaket { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public string Gambar { get; set; } = string.Empty;
        public int TotalPorsi { get; set; }
        public int JumlahPesanan { get; set; }
        public List<string> CatatanPesanan { get; set; } = new();
    }

    public class ManifesPesananRingkasViewModel
    {
        public int PesananId { get; set; }
        public string NomorPesanan { get; set; } = string.Empty;
        public string NamaPelanggan { get; set; } = string.Empty;
        public string Telepon { get; set; } = string.Empty;
        public string JamPengantaran { get; set; } = string.Empty;
        public int TotalPorsi { get; set; }
        public string AlamatPengiriman { get; set; } = string.Empty;
        public string StatusPesanan { get; set; } = string.Empty;
        public string? Catatan { get; set; }
    }
}
