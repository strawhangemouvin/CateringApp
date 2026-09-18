using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;

namespace CateringApp.Services.Interface;

public interface ICateringService
{
    DashboardViewModel GetDashboardSummary();
    CustomerDashboardViewModel GetCustomerDashboardSummary(int penggunaId);
    PesananPagedResult GetPagedPesananList(int? penggunaId, string? search, string? status, int? kategoriId, DateTime? tanggal, string? sort, int page, int size);
    
    List<PaketMenu> GetAllPaket();
    PaketMenu? GetPaketById(int id);
    void CreatePaket(PaketMenu paket);
    void UpdatePaket(PaketMenu paket);
    void SoftDeletePaket(int id);

    List<KategoriMenu> GetAllKategori();
    KategoriMenu? GetKategoriById(int id);
    void CreateKategori(KategoriMenu kategori);
    void UpdateKategori(KategoriMenu kategori);
    void SoftDeleteKategori(int id);
        
    List<Pengguna> GetAllPengguna();
    Pengguna? GetPenggunaById(int id);
    void CreatePengguna(Pengguna pengguna);
    void UpdatePengguna(Pengguna pengguna);
    void SoftDeletePengguna(int id);
    List<Peran> GetAllPeran();

    List<Pesanan> GetAllPesanan(int? penggunaId = null);
    Pesanan? GetPesananById(int id);
    int BuatPesanan(int penggunaId, PemesananViewModel model);
    int BuatPesananDariKeranjang(int penggunaId, List<CartItem> cartItems, CheckoutViewModel model);
    void UpdateStatusPesanan(int pesananId, string status);
    void SoftDeletePesanan(int id);

    void UploadBuktiBayar(UploadPembayaranViewModel model, string filePath);
    void VerifikasiPembayaran(int pesananId, string status);
    void KonfirmasiPembayaranOtomatis(int pesananId, string metodePembayaran);
    void AjukanRefund(int pesananId, string bank, string noRekening, string atasNama);
    void InputRekeningRefund(int pesananId, string bank, string noRekening, string atasNama);
    void KonfirmasiRefund(int pesananId, string buktiTfPath);
    (bool IsAllowed, string Reason) CekKelayakanRefund(int pesananId);

    int GetTotalPorsiByTanggal(DateTime tanggal);
    ManifesDapurViewModel GetManifesDapur(DateTime tanggal);
    List<NotifikasiItemViewModel> GetNotifikasiList(string role, int? userId);
}
