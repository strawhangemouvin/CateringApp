using CateringApp.Filters;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Interface;
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

    public IActionResult Create(int? paketId = null)
    {
        ViewBag.PaketId = new SelectList(_service.GetAllPaket(), "PaketId", "NamaPaket", paketId);
        return View(new PemesananViewModel { PaketId = paketId ?? 0 });
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PemesananViewModel model)
    {
        if (ModelState.IsValid)
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            int pesananId = _service.BuatPesanan(userId, model);
            TempData["Success"] = "Pesanan berhasil dibuat. Silakan upload bukti pembayaran.";
            return RedirectToAction("Bayar", new { id = pesananId });
        }
        ViewBag.PaketId = new SelectList(_service.GetAllPaket(), "PaketId", "NamaPaket", model.PaketId);
        return View(model);
    }

    public IActionResult Details(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();
        return View(pesanan);
    }

    public IActionResult Bayar(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

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

    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public IActionResult Verifikasi(int id, string status)
    {
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

        if (pesanan.StatusPesanan == "Selesai" || pesanan.StatusPesanan == "Dibatalkan" || pesanan.StatusPesanan == "Refund Selesai")
        {
            TempData["Error"] = "Pesanan ini sudah selesai atau telah dibatalkan.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        if (string.IsNullOrWhiteSpace(namaBank) || string.IsNullOrWhiteSpace(noRekening) || string.IsNullOrWhiteSpace(atasNama))
        {
            TempData["Error"] = "Mohon lengkapi seluruh informasi rekening untuk pengembalian dana (Refund).";
            return RedirectToAction("Details", new { id = pesananId });
        }

        _service.AjukanRefund(pesananId, namaBank.Trim(), noRekening.Trim(), atasNama.Trim());
        TempData["Success"] = "Pengajuan pembatalan berhasil diajukan. Dana Anda sedang dalam antrean pengembalian (Refund).";
        return RedirectToAction("Details", new { id = pesananId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public async Task<IActionResult> KonfirmasiRefund(int pesananId, IFormFile fileBuktiTf)
    {
        var pesanan = _service.GetPesananById(pesananId);
        if (pesanan == null) return NotFound();

        if (fileBuktiTf != null && fileBuktiTf.Length > 0)
        {
            string ext = Path.GetExtension(fileBuktiTf.FileName).ToLower();
            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            if (allowedExt.Contains(ext))
            {
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
            TempData["Error"] = "Format berkas bukti transfer harus berupa Gambar (JPG, PNG, WEBP) atau PDF.";
            return RedirectToAction("Details", new { id = pesananId });
        }

        TempData["Error"] = "Harap unggah berkas bukti transfer pengembalian dana.";
        return RedirectToAction("Details", new { id = pesananId });
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
            TempData["Error"] = "Karyawan tidak memiliki wewenang untuk membatalkan pesanan.";
            return RedirectToAction("Details", new { id = id });
        }

        _service.SoftDeletePesanan(id);
        TempData["Success"] = "Pesanan berhasil dibatalkan/dihapus.";
        return RedirectToAction("Index");
    }
}
