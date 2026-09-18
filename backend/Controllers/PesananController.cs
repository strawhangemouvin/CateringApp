using CateringApp.Filters;
using CateringApp.Helpers;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;

namespace CateringApp.Controllers;

[SessionAuthorize]
public class PesananController : Controller
{
    private readonly ICateringService _service;
    private readonly IWebHostEnvironment _env;

    public PesananController(ICateringService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    public IActionResult Index(string? search, string? status, int? kategoriId, System.DateTime? tanggal, string? sort, int page = 1, int size = 5)
    {
        string? role = HttpContext.Session.GetString("Role");
        int? userId = (role == "Pemilik Toko" || role == "Karyawan") ? null : HttpContext.Session.GetInt32("UserId");

        var pagedResult = _service.GetPagedPesananList(userId, search, status, kategoriId, tanggal, sort, page, size);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.SelectedKategoriId = kategoriId;
        ViewBag.Tanggal = tanggal?.ToString("yyyy-MM-dd");
        ViewBag.Sort = sort ?? "terbaru";
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        ViewBag.TotalPages = pagedResult.TotalPages;
        ViewBag.TotalRecords = pagedResult.TotalRecords;

        ViewBag.KategoriList = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", kategoriId);

        return View(pagedResult.Items);
    }

    [AllowAnonymous]
    public IActionResult Katalog(string? search, int? kategoriId, int page = 1)
    {
        var list = _service.GetAllPaket();

        if (!string.IsNullOrEmpty(search))
        {
            string s = search.ToLower();
            list = list.Where(p => p.NamaPaket.ToLower().Contains(s) || (p.DeskripsiMenu != null && p.DeskripsiMenu.ToLower().Contains(s))).ToList();
        }

        if (kategoriId.HasValue)
        {
            list = list.Where(p => p.KategoriId == kategoriId.Value).ToList();
        }

        int pageSize = 6;
        int totalItems = list.Count;
        var pagedList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Search = search;
        ViewBag.SelectedKategoriId = kategoriId;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", kategoriId);
        
        return View(pagedList);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetPaketDetail(int id)
    {
        var paket = _service.GetPaketById(id);
        if (paket == null) return NotFound();

        string categoryName = paket.Kategori?.NamaKategori ?? "Menu Pilihan";
        string photoUrl = CateringApp.Helpers.MenuImageHelper.GetGambarUrl(paket.NamaPaket, paket.Gambar, categoryName);

        return Json(new
        {
            paketId = paket.PaketId,
            namaPaket = paket.NamaPaket,
            kategori = categoryName,
            harga = paket.Harga,
            hargaFormat = "Rp " + paket.Harga.ToString("N0"),
            deskripsi = paket.DeskripsiMenu ?? "Sajian istimewa kaya cita rasa tradisional khas Catering Mimi Saripah yang higienis, halal, dan lezat.",
            gambar = photoUrl
        });
    }

    [AllowAnonymous]
    public IActionResult DetailMenu(int id)
    {
        var paket = _service.GetPaketById(id);
        if (paket == null) return NotFound();

        return View(paket);
    }
    [HttpGet]
    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public IActionResult Create(int? paketId = null)
    {
        var customers = _service.GetAllPengguna()
            .Where(p => p.Peran?.NamaPeran == "User" || p.PeranId == 3)
            .OrderBy(p => p.NamaLengkap)
            .ToList();

        ViewBag.CustomerList = new SelectList(customers, "PenggunaId", "NamaLengkap");
        ViewBag.PaketList = new SelectList(_service.GetAllPaket(), "PaketId", "NamaPaket", paketId);

        var model = new PemesananViewModel
        {
            PaketId = paketId ?? 0,
            TanggalPengiriman = DateTime.Today.AddDays(2),
            JamPengantaran = "10:00 - 12:00",
            JumlahPorsi = 10
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public IActionResult Create(PemesananViewModel model)
    {
        // Jika tidak ada pelanggan terdaftar dipilih, nama manual wajib diisi
        bool isPelangganTerdaftar = model.PenggunaId.HasValue && model.PenggunaId.Value > 0;
        if (!isPelangganTerdaftar && string.IsNullOrWhiteSpace(model.NamaPemesanManual))
        {
            ModelState.AddModelError("NamaPemesanManual", "Nama pemesan wajib diisi jika pelanggan belum punya akun (tamu / walk-in / WA).");
        }

        if (!isPelangganTerdaftar && !string.IsNullOrWhiteSpace(model.TeleponPemesanManual))
        {
            var (isPhoneValid, phoneErr) = ValidationHelper.ValidateNomorTelepon(model.TeleponPemesanManual, isRequired: false);
            if (!isPhoneValid)
            {
                ModelState.AddModelError("TeleponPemesanManual", phoneErr!);
            }
        }

        if (model.JumlahPorsi < 10)
        {
            ModelState.AddModelError("JumlahPorsi", "Minimal pemesanan paket katering adalah 10 porsi.");
        }

        if (ModelState.IsValid)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int pesananId = _service.BuatPesanan(currentUserId, model);
            string namaPemesan = isPelangganTerdaftar ? "" : $" atas nama {model.NamaPemesanManual}";
            TempData["Success"] = $"Pesanan manual{namaPemesan} berhasil dicatat (Nomor Pesanan #{pesananId})!";
            return RedirectToAction("Details", new { id = pesananId });
        }

        var customers = _service.GetAllPengguna()
            .Where(p => p.Peran?.NamaPeran == "User" || p.PeranId == 3)
            .OrderBy(p => p.NamaLengkap)
            .ToList();

        ViewBag.CustomerList = new SelectList(customers, "PenggunaId", "NamaLengkap", model.PenggunaId);
        ViewBag.PaketList = new SelectList(_service.GetAllPaket(), "PaketId", "NamaPaket", model.PaketId);
        return View(model);
    }

    public IActionResult Details(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role == "User" && pesanan.PenggunaId != currentUserId)
        {
            TempData["Error"] = "Anda tidak memiliki izin untuk mengakses pesanan ini.";
            return RedirectToAction("Index");
        }

        return View(pesanan);
    }

    public IActionResult Bayar(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role == "User" && pesanan.PenggunaId != currentUserId)
        {
            TempData["Error"] = "Anda tidak memiliki izin untuk mengakses pesanan ini.";
            return RedirectToAction("Index");
        }

        var model = new UploadPembayaranViewModel
        {
            PesananId = pesanan.PesananId,
            NomorPesanan = pesanan.NomorPesanan,
            TotalBayar = pesanan.TotalBayar
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bayar(UploadPembayaranViewModel model)
    {
        if (model.FileBukti != null && model.FileBukti.Length > 0)
        {
            string ext = Path.GetExtension(model.FileBukti.FileName).ToLower();
            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            if (allowedExt.Contains(ext))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"PAY_{model.PesananId}_{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FileBukti.CopyToAsync(fileStream);
                }

                _service.UploadBuktiBayar(model, $"/uploads/{uniqueFileName}");
                TempData["Success"] = "Bukti pembayaran berhasil diunggah. Mohon tunggu verifikasi admin.";
                return RedirectToAction("Details", new { id = model.PesananId });
            }
            ModelState.AddModelError("FileBukti", "Format file harus berupa Gambar (JPG, JPEG, PNG, WEBP) atau Dokumen PDF.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult KonfirmasiPembayaranOtomatis(int pesananId, string metodePembayaran)
    {
        var pesanan = _service.GetPesananById(pesananId);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role == "User" && pesanan.PenggunaId != currentUserId)
        {
            TempData["Error"] = "Anda tidak memiliki izin untuk mengakses pesanan ini.";
            return RedirectToAction("Index");
        }

        string method = string.IsNullOrWhiteSpace(metodePembayaran) ? "BCA Virtual Account" : metodePembayaran.Trim();

        _service.KonfirmasiPembayaranOtomatis(pesananId, method);

        TempData["Success"] = $"Pembayaran via {method} berhasil diverifikasi! Pesanan Anda telah lunas dan kini sedang diproses dapur.";
        return RedirectToAction("Details", new { id = pesananId });
    }

    [SessionAuthorize("Pemilik Toko")]
    public IActionResult Verifikasi(int id, string status)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        if (pesanan.StatusPesanan == "Dikirim" || pesanan.StatusPesanan == "Selesai")
        {
            TempData["Error"] = "Status verifikasi pembayaran tidak dapat diubah karena pesanan sudah dalam proses pengiriman kurir atau sudah selesai.";
            return RedirectToAction("Details", new { id });
        }

        _service.VerifikasiPembayaran(id, status);
        TempData["Success"] = $"Pembayaran berhasil diverifikasi sebagai '{status}'.";
        return RedirectToAction("Details", new { id });
    }

    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public IActionResult UpdateStatus(int id, string status)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        string current = pesanan.StatusPesanan;

        if (current == "Refund Selesai")
        {
            TempData["Error"] = "Pengembalian dana (Refund) untuk pesanan ini telah selesai dan transaksi ditutup. Status alur tidak dapat diubah lagi.";
            return RedirectToAction("Details", new { id });
        }

        if (current == "Menunggu Refund")
        {
            TempData["Error"] = "Pesanan sedang dalam proses pengajuan refund. Silakan proses transfer dan konfirmasi bukti transfer melalui kartu Pengembalian Dana.";
            return RedirectToAction("Details", new { id });
        }

        if (current == "Selesai")
        {
            TempData["Error"] = "Pesanan ini sudah SELESAI dan pesanan telah diterima pelanggan. Status tidak dapat diubah lagi.";
            return RedirectToAction("Details", new { id });
        }

        if (current == "Dibatalkan" || current == "Batal")
        {
            TempData["Error"] = "Pesanan ini sudah DIBATALKAN. Status tidak dapat diubah lagi.";
            return RedirectToAction("Details", new { id });
        }

        if (status == "Dibatalkan" && (current == "Dikirim" || current == "Selesai" || current == "Menunggu Refund" || current == "Refund Selesai"))
        {
            TempData["Error"] = "Pesanan yang sedang dalam alur refund, pengiriman kurir, atau sudah selesai tidak dapat dibatalkan.";
            return RedirectToAction("Details", new { id });
        }

        if (status == "Pending")
        {
            TempData["Error"] = "Tidak dapat mengembalikan status pesanan yang sedang berjalan kembali ke 'Pending'.";
            return RedirectToAction("Details", new { id });
        }

        var requiresPaymentStatuses = new[] { "Diproses", "Dikirim", "Selesai" };
        if (requiresPaymentStatuses.Contains(status))
        {
            if (pesanan.Pembayaran == null || pesanan.Pembayaran.StatusVerifikasi != "Valid")
            {
                TempData["Error"] = $"Pesanan tidak dapat diubah ke status '{status}' karena pembayaran belum diverifikasi (Valid).";
                return RedirectToAction("Details", new { id });
            }
        }

        if (status == "Dikirim" && current != "Diproses")
        {
            TempData["Error"] = "Pesanan harus dalam status 'Diproses (Masak)' terlebih dahulu sebelum dapat diubah menjadi 'Dikirim'.";
            return RedirectToAction("Details", new { id });
        }

        if (status == "Selesai" && current != "Dikirim")
        {
            TempData["Error"] = "Pesanan harus dalam status 'Dikirim' terlebih dahulu sebelum dapat diselesaikan (Selesai).";
            return RedirectToAction("Details", new { id });
        }

        if (status == "Dibatalkan" && pesanan.Pembayaran != null && pesanan.Pembayaran.StatusVerifikasi == "Valid")
        {
            status = "Menunggu Refund";
            _service.UpdateStatusPesanan(id, status);
            TempData["Warning"] = "Pesanan telah dibatalkan. Karena pembayaran pelanggan telah terverifikasi lunas, status otomatis dialihkan ke 'Menunggu Refund' agar Pemilik Toko (Owner) dapat memproses transfer pengembalian dana ke rekening pelanggan.";
            return RedirectToAction("Details", new { id });
        }

        _service.UpdateStatusPesanan(id, status);
        TempData["Success"] = $"Status pesanan berhasil diperbarui menjadi '{status}'.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjukanRefund(int pesananId, string namaBank, string noRekening, string atasNama)
    {
        var pesanan = _service.GetPesananById(pesananId);
        if (pesanan == null) return NotFound();

        int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
        string? role = HttpContext.Session.GetString("Role");

        if (role == "User" && pesanan.PenggunaId != currentUserId)
        {
            TempData["Error"] = "Anda tidak memiliki hak untuk membatalkan pesanan ini.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        // Cek kelayakan refund dan cutoff H-2
        var (isAllowed, reason) = _service.CekKelayakanRefund(pesananId);
        if (!isAllowed)
        {
            TempData["Error"] = reason;
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (string.IsNullOrWhiteSpace(namaBank) || string.IsNullOrWhiteSpace(noRekening) || string.IsNullOrWhiteSpace(atasNama))
        {
            TempData["Error"] = "Mohon lengkapi seluruh informasi rekening untuk pengembalian dana (Refund).";
            return RedirectToAction("Details", new { id = pesananId });
        }

        namaBank = namaBank.Trim();
        noRekening = noRekening.Trim();
        atasNama = atasNama.Trim();

        if (namaBank.Length > 50)
        {
            TempData["Error"] = "Nama Bank / E-Wallet tidak boleh melebihi 50 karakter.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (noRekening.Length < 8 || noRekening.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(noRekening, @"^[0-9]+$"))
        {
            TempData["Error"] = "Nomor Rekening / No. HP E-Wallet harus berupa 8 hingga 20 digit angka.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (atasNama.Length < 3 || atasNama.Length > 70)
        {
            TempData["Error"] = "Nama Pemilik Rekening harus antara 3 hingga 70 karakter.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        try
        {
            _service.AjukanRefund(pesananId, namaBank, noRekening, atasNama);
            TempData["Success"] = "Pengajuan pembatalan berhasil diajukan. Dana Anda sedang dalam antrean pengembalian (Refund).";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Gagal memproses pengajuan refund: " + ex.Message;
        }

        return RedirectToAction("Details", new { id = pesananId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InputRekeningRefund(int pesananId, string namaBank, string noRekening, string atasNama)
    {
        var pesanan = _service.GetPesananById(pesananId);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (role == "Karyawan")
        {
            TempData["Error"] = "Karyawan tidak memiliki wewenang untuk menginput atau mengubah rekening pengembalian dana.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (role == "User" && pesanan.PenggunaId != userId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(namaBank) || string.IsNullOrWhiteSpace(noRekening) || string.IsNullOrWhiteSpace(atasNama))
        {
            TempData["Error"] = "Mohon lengkapi seluruh informasi rekening untuk pengembalian dana (Refund).";
            return RedirectToAction("Details", new { id = pesananId });
        }

        namaBank = namaBank.Trim();
        noRekening = noRekening.Trim();
        atasNama = atasNama.Trim();

        if (noRekening.Length < 8 || noRekening.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(noRekening, @"^[0-9]+$"))
        {
            TempData["Error"] = "Nomor Rekening / No. HP E-Wallet harus berupa 8 hingga 20 digit angka.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        _service.InputRekeningRefund(pesananId, namaBank, noRekening, atasNama);
        TempData["Success"] = "Data rekening pengembalian dana berhasil disimpan. Pemilik Toko (Owner) telah menerima notifikasi untuk memproses transfer dana.";
        return RedirectToAction("Details", new { id = pesananId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Pemilik Toko")]
    public async Task<IActionResult> KonfirmasiRefund(int pesananId, IFormFile fileBuktiTf)
    {
        var pesanan = _service.GetPesananById(pesananId);
        if (pesanan == null) return NotFound();

        if (fileBuktiTf == null || fileBuktiTf.Length == 0)
        {
            TempData["Error"] = "Wajib mengunggah berkas bukti transfer pengembalian dana sebelum melakukan konfirmasi selesai.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (fileBuktiTf.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "Ukuran berkas bukti transfer terlalu besar. Maksimal ukuran berkas adalah 5 MB.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        string ext = Path.GetExtension(fileBuktiTf.FileName).ToLower();
        var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        if (!allowedExt.Contains(ext))
        {
            TempData["Error"] = "Format berkas bukti transfer harus berupa Gambar (JPG, PNG, WEBP) atau PDF.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "refund");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = $"REFUND_{pesananId}_{Guid.NewGuid()}{ext}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await fileBuktiTf.CopyToAsync(fileStream);
        }

        _service.KonfirmasiRefund(pesananId, $"/uploads/refund/{uniqueFileName}");
        TempData["Success"] = "Pengembalian dana (Refund) berhasil dikonfirmasi dan bukti transfer telah tersimpan.";
        return RedirectToAction("Details", new { id = pesananId });
    }

    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public IActionResult ManifesDapur(DateTime? tanggal)
    {
        DateTime targetDate = tanggal ?? DateTime.Today.AddDays(1);
        var model = _service.GetManifesDapur(targetDate);
        ViewBag.SelectedDate = targetDate;
        return View(model);
    }

    public IActionResult Delete(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role == "User")
        {
            if (pesanan.PenggunaId != currentUserId || pesanan.StatusPesanan != "Pending")
            {
                TempData["Error"] = "Anda hanya dapat membatalkan pesanan milik sendiri yang berstatus Pending.";
                return RedirectToAction("Details", new { id = id });
            }
        }
        else if (role == "Karyawan")
        {
            TempData["Error"] = "Karyawan tidak memiliki wewenang untuk membatalkan atau menghapus pesanan.";
            return RedirectToAction("Details", new { id = id });
        }

        if (pesanan.StatusPesanan == "Dikirim" || pesanan.StatusPesanan == "Selesai" || pesanan.StatusPesanan == "Menunggu Refund" || pesanan.StatusPesanan == "Refund Selesai")
        {
            TempData["Error"] = "Pesanan yang sedang dalam proses pengiriman, sudah selesai, atau dalam alur pengembalian dana (refund) tidak dapat dibatalkan atau dihapus.";
            return RedirectToAction("Details", new { id = id });
        }

        _service.SoftDeletePesanan(id);
        TempData["Success"] = "Pesanan berhasil dibatalkan/dihapus.";
        return RedirectToAction("Index");
    }
}
