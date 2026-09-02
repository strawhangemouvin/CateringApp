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
            if (ext == ".jpg" || ext == ".png" || ext == ".pdf")
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
            ModelState.AddModelError("FileBukti", "Format file harus JPG, PNG, atau PDF.");
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
        _service.UpdateStatusPesanan(id, status);
        TempData["Success"] = $"Status pesanan berhasil diperbarui menjadi '{status}'.";
        return RedirectToAction("Details", new { id });
    }

    public IActionResult Delete(int id)
    {
        var pesanan = _service.GetPesananById(id);
        if (pesanan == null) return NotFound();

        string? role = HttpContext.Session.GetString("Role");
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role == "User")
        {
            // Pelanggan hanya bisa cancel pesanannya sendiri dan statusnya harus Pending
            if (pesanan.PenggunaId != currentUserId || pesanan.StatusPesanan != "Pending")
            {
                TempData["Error"] = "Anda hanya dapat membatalkan pesanan milik sendiri yang berstatus Pending.";
                return RedirectToAction("Details", new { id = id });
            }
        }
        else if (role == "Karyawan")
        {
            // Karyawan tidak berhak membatalkan pesanan
            TempData["Error"] = "Karyawan tidak memiliki wewenang untuk membatalkan pesanan.";
            return RedirectToAction("Details", new { id = id });
        }
        // Pemilik Toko berhak membatalkan pesanan apa saja

        _service.SoftDeletePesanan(id);
        TempData["Success"] = "Pesanan berhasil dibatalkan/dihapus.";
        return RedirectToAction("Index");
    }
}