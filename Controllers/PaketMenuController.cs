using CateringApp.Filters;
using CateringApp.Models.Entity;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CateringApp.Controllers
{
    [SessionAuthorize("Pemilik Toko", "Karyawan")]
    public class PaketMenuController : Controller
    {
        private readonly ICateringService _service;
        private readonly IWebHostEnvironment _env;

        public PaketMenuController(ICateringService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        public IActionResult Index(string search, int? kategoriId, string sort, int page = 1, int size = 5)
        {
            var list = _service.GetAllPaket();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                list = list.Where(p => p.NamaPaket.ToLower().Contains(s) || (p.DeskripsiMenu != null && p.DeskripsiMenu.ToLower().Contains(s))).ToList();
            }

            // Filter
            if (kategoriId.HasValue)
            {
                list = list.Where(p => p.KategoriId == kategoriId.Value).ToList();
            }

            // Sort
            sort = string.IsNullOrEmpty(sort) ? "terbaru" : sort;
            list = sort switch
            {
                "A-Z" => list.OrderBy(p => p.NamaPaket).ToList(),
                "Z-A" => list.OrderByDescending(p => p.NamaPaket).ToList(),
                "termurah" => list.OrderBy(p => p.Harga).ToList(),
                "termahal" => list.OrderByDescending(p => p.Harga).ToList(),
                "terlama" => list.OrderBy(p => p.CreatedAt ?? DateTime.MinValue).ThenBy(p => p.PaketId).ToList(),
                _ => list.OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue).ThenByDescending(p => p.PaketId).ToList(),
            };

            // Pagination
            int total = list.Count;
            var pagedList = list.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.Search = search;
            ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", kategoriId);
            ViewBag.SelectedKategori = kategoriId;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;
            ViewBag.TotalRecords = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / size);

            return View(pagedList);
        }

        public IActionResult Create()
        {
            ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaketMenu model, IFormFile? FileGambar)
        {
            if (ModelState.IsValid)
            {
                if (FileGambar != null && FileGambar.Length > 0)
                {
                    string ext = Path.GetExtension(FileGambar.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "menu");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = $"MENU_{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await FileGambar.CopyToAsync(fileStream);
                        }

                        model.Gambar = $"/uploads/menu/{uniqueFileName}";
                    }
                }

                _service.CreatePaket(model);
                TempData["Success"] = "Paket menu berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", model.KategoriId);
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var data = _service.GetPaketById(id);
            if (data == null) return NotFound();
            ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", data.KategoriId);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaketMenu model, IFormFile? FileGambar)
        {
            if (ModelState.IsValid)
            {
                if (FileGambar != null && FileGambar.Length > 0)
                {
                    string ext = Path.GetExtension(FileGambar.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "menu");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = $"MENU_{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await FileGambar.CopyToAsync(fileStream);
                        }

                        model.Gambar = $"/uploads/menu/{uniqueFileName}";
                    }
                }

                _service.UpdatePaket(model);
                TempData["Success"] = "Paket menu berhasil diubah.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.KategoriId = new SelectList(_service.GetAllKategori(), "KategoriId", "NamaKategori", model.KategoriId);
            return View(model);
        }

        public IActionResult Details(int id)
        {
            var data = _service.GetPaketById(id);
            if (data == null) return NotFound();
            return View(data);
        }

        public IActionResult Delete(int id)
        {
            _service.SoftDeletePaket(id);
            TempData["Success"] = "Paket menu berhasil dihapus (Soft Delete).";
            return RedirectToAction(nameof(Index));
        }
    }
}
