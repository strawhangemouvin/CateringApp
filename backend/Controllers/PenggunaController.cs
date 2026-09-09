using CateringApp.Filters;
using CateringApp.Models.Entity;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace CateringApp.Controllers
{
    [SessionAuthorize("Pemilik Toko")]
    public class PenggunaController : Controller
    {
        private readonly ICateringService _service;

        public PenggunaController(ICateringService service)
        {
            _service = service;
        }

        public IActionResult Index(string search, int? peranId, string sort, int page = 1, int size = 5)
        {
            var list = _service.GetAllPengguna();

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                list = list.Where(u => u.NamaLengkap.ToLower().Contains(s) || u.Username.ToLower().Contains(s) || u.Email.ToLower().Contains(s)).ToList();
            }

            if (peranId.HasValue)
            {
                list = list.Where(u => u.PeranId == peranId.Value).ToList();
            }

            sort = string.IsNullOrEmpty(sort) ? "terbaru" : sort;
            list = sort switch
            {
                "A-Z" => list.OrderBy(u => u.NamaLengkap).ToList(),
                "Z-A" => list.OrderByDescending(u => u.NamaLengkap).ToList(),
                "terlama" => list.OrderBy(u => u.CreatedAt ?? DateTime.MinValue).ThenBy(u => u.PenggunaId).ToList(),
                _ => list.OrderByDescending(u => u.CreatedAt ?? DateTime.MinValue).ThenByDescending(u => u.PenggunaId).ToList(),
            };

            int total = list.Count;
            var pagedList = list.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.Search = search;
            ViewBag.PeranId = new SelectList(_service.GetAllPeran(), "PeranId", "NamaPeran", peranId);
            ViewBag.SelectedPeran = peranId;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;
            ViewBag.TotalRecords = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / size);

            return View(pagedList);
        }

        public IActionResult Create()
        {
            ViewBag.PeranId = new SelectList(_service.GetAllPeran(), "PeranId", "NamaPeran");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pengguna model)
        {
            if (ModelState.IsValid)
            {
                _service.CreatePengguna(model);
                TempData["Success"] = "Pengguna berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PeranId = new SelectList(_service.GetAllPeran(), "PeranId", "NamaPeran", model.PeranId);
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var data = _service.GetPenggunaById(id);
            if (data == null) return NotFound();
            ViewBag.PeranId = new SelectList(_service.GetAllPeran(), "PeranId", "NamaPeran", data.PeranId);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Pengguna model)
        {
            if (ModelState.IsValid)
            {
                _service.UpdatePengguna(model);
                TempData["Success"] = "Pengguna berhasil diubah.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PeranId = new SelectList(_service.GetAllPeran(), "PeranId", "NamaPeran", model.PeranId);
            return View(model);
        }

        public IActionResult Details(int id)
        {
            var data = _service.GetPenggunaById(id);
            if (data == null) return NotFound();
            return View(data);
        }

        public IActionResult Delete(int id)
        {
            _service.SoftDeletePengguna(id);
            TempData["Success"] = "Pengguna berhasil dihapus (Soft Delete).";
            return RedirectToAction(nameof(Index));
        }
    }
}
