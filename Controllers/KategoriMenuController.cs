using CateringApp.Filters;
using CateringApp.Models.Entity;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CateringApp.Controllers
{
    [SessionAuthorize("Pemilik Toko")]
    public class KategoriMenuController : Controller
    {
        private readonly ICateringService _service;

        public KategoriMenuController(ICateringService service)
        {
            _service = service;
        }

        public IActionResult Index(string search, string sort, int page = 1, int size = 5)
        {
            var list = _service.GetAllKategori();

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                list = list.Where(c => c.NamaKategori.ToLower().Contains(s) || (c.Deskripsi != null && c.Deskripsi.ToLower().Contains(s))).ToList();
            }

            sort = string.IsNullOrEmpty(sort) ? "terbaru" : sort;
            list = sort switch
            {
                "A-Z" => list.OrderBy(c => c.NamaKategori).ToList(),
                "Z-A" => list.OrderByDescending(c => c.NamaKategori).ToList(),
                "terlama" => list.OrderBy(c => c.CreatedAt ?? DateTime.MinValue).ThenBy(c => c.KategoriId).ToList(),
                _ => list.OrderByDescending(c => c.CreatedAt ?? DateTime.MinValue).ThenByDescending(c => c.KategoriId).ToList(),
            };

            int total = list.Count;
            var pagedList = list.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;
            ViewBag.TotalRecords = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / size);

            return View(pagedList);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KategoriMenu model)
        {
            if (ModelState.IsValid)
            {
                _service.CreateKategori(model);
                TempData["Success"] = "Kategori menu berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var data = _service.GetKategoriById(id);
            if (data == null) return NotFound();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KategoriMenu model)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateKategori(model);
                TempData["Success"] = "Kategori menu berhasil diubah.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Details(int id)
        {
            var data = _service.GetKategoriById(id);
            if (data == null) return NotFound();
            return View(data);
        }

        public IActionResult Delete(int id)
        {
            _service.SoftDeleteKategori(id);
            TempData["Success"] = "Kategori menu berhasil dihapus (Soft Delete).";
            return RedirectToAction(nameof(Index));
        }
    }
}
