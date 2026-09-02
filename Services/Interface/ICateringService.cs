using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;

namespace CateringApp.Services.Interface;

public interface ICateringService
{
    DashboardViewModel GetDashboardSummary();
    CustomerDashboardViewModel GetCustomerDashboardSummary(int penggunaId);
    PesananPagedResult GetPagedPesananList(int? penggunaId, string? search, string? status, int? kategoriId, DateTime? tanggal, string? sort, int page, int size);
    
    // Paket Menu CRUD
    List<PaketMenu> GetAllPaket();
    PaketMenu? GetPaketById(int id);
    void CreatePaket(PaketMenu paket);
    void UpdatePaket(PaketMenu paket);
    void SoftDeletePaket(int id);

    // Kategori Menu CRUD
    List<KategoriMenu> GetAllKategori();
    KategoriMenu? GetKategoriById(int id);
    void CreateKategori(KategoriMenu kategori);
    void UpdateKategori(KategoriMenu kategori);
    void SoftDeleteKategori(int id);
        
    // Pengguna CRUD
    List<Pengguna> GetAllPengguna();
    Pengguna? GetPenggunaById(int id);
    void CreatePengguna(Pengguna pengguna);
    void UpdatePengguna(Pengguna pengguna);
    void SoftDeletePengguna(int id);
    List<Peran> GetAllPeran();

    // Pesanan & Transaksi
    List<Pesanan> GetAllPesanan(int? penggunaId = null);
    Pesanan? GetPesananById(int id);
    int BuatPesanan(int penggunaId, PemesananViewModel model);
    int BuatPesananDariKeranjang(int penggunaId, List<CartItem> cartItems, CheckoutViewModel model);
    void UpdateStatusPesanan(int pesananId, string status);
    void SoftDeletePesanan(int id);

    // Pembayaran
    void UploadBuktiBayar(UploadPembayaranViewModel model, string filePath);
    void VerifikasiPembayaran(int pesananId, string status);
}