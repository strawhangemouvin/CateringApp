using CateringApp.Filters;
using CateringApp.Helpers;
using CateringApp.Models.Entity;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace CateringApp.Controllers
{
    [SessionAuthorize("Pemilik Toko")]
    public class PenggunaController(ICateringService service) : Controller
    {
        private readonly ICateringService _service = service;

        public IActionResult Index(string search, int? peranId, string sort, int page = 1, int size = 5)
        {
            var list = _service.GetAllPengguna();

            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(u => u.NamaLengkap.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
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
            var perans = _service.GetAllPeran().Select(p => new {
                p.PeranId,
                NamaPeran = p.NamaPeran == "User" ? "Pelanggan" : p.NamaPeran
            }).ToList();
            ViewBag.PeranId = new SelectList(perans, "PeranId", "NamaPeran", peranId);
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
            // Pemilik Toko hanya ada 1 akun utama dan tidak boleh ditambahkan lagi
            var availableRoles = _service.GetAllPeran()
                .Where(p => p.NamaPeran != "Pemilik Toko" && p.PeranId != 1)
                .Select(p => new {
                    p.PeranId,
                    NamaPeran = p.NamaPeran == "User" ? "Pelanggan" : p.NamaPeran
                })
                .ToList();
            ViewBag.PeranId = new SelectList(availableRoles, "PeranId", "NamaPeran");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pengguna model)
        {
            // Owner hanya boleh 1
            var roleObj = _service.GetAllPeran().FirstOrDefault(p => p.PeranId == model.PeranId);
            if (roleObj == null || roleObj.NamaPeran == "Pemilik Toko" || model.PeranId == 1)
            {
                ModelState.AddModelError("PeranId", "Peran Pemilik Toko hanya ada 1 akun utama dan tidak dapat ditambahkan lagi.");
            }

            // Validasi Nama Lengkap
            if (string.IsNullOrWhiteSpace(model.NamaLengkap) || model.NamaLengkap.Trim().Length < 3)
            {
                ModelState.AddModelError("NamaLengkap", "Nama lengkap wajib diisi minimal 3 karakter.");
            }

            // Validasi Username & Keunikan
            var (isUserValid, userErr) = ValidationHelper.ValidateUsername(model.Username);
            if (!isUserValid)
            {
                ModelState.AddModelError("Username", userErr!);
            }
            else
            {
                string cleanUser = model.Username.Trim();
                if (_service.GetAllPengguna().Any(u => u.DeletedAt == null && string.Equals(u.Username, cleanUser, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Username", "Username sudah digunakan. Silakan pilih username lain.");
                }
            }

            // Validasi Password Baru
            var (isPassValid, passErr) = ValidationHelper.ValidatePassword(model.PasswordHash, isRequired: true);
            if (!isPassValid)
            {
                ModelState.AddModelError("PasswordHash", passErr!);
            }

            // Validasi Email & Keunikan (1 email 1 akun)
            var (isEmailValid, emailErr) = ValidationHelper.ValidateEmail(model.Email, isRequired: true);
            if (!isEmailValid)
            {
                ModelState.AddModelError("Email", emailErr!);
            }
            else
            {
                string cleanEmail = model.Email.Trim();
                if (_service.GetAllPengguna().Any(u => u.DeletedAt == null && string.Equals(u.Email, cleanEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Email", "Email sudah terdaftar. Setiap akun pengguna harus menggunakan email yang unik.");
                }
            }

            // Validasi Nomor Telepon
            var (isPhoneValid, phoneErr) = ValidationHelper.ValidateNomorTelepon(model.NomorTelepon, isRequired: true);
            if (!isPhoneValid)
            {
                ModelState.AddModelError("NomorTelepon", phoneErr!);
            }

            // Validasi Alamat
            if (string.IsNullOrWhiteSpace(model.Alamat) || model.Alamat.Trim().Length < 5)
            {
                ModelState.AddModelError("Alamat", "Alamat lengkap wajib diisi minimal 5 karakter.");
            }

            if (ModelState.IsValid)
            {
                model.NamaLengkap = model.NamaLengkap.Trim();
                model.Username = model.Username.Trim();
                model.Email = model.Email.Trim().ToLower();
                model.NomorTelepon = model.NomorTelepon?.Trim();
                model.Alamat = model.Alamat?.Trim();

                _service.CreatePengguna(model);
                TempData["Success"] = $"Pengguna '{model.NamaLengkap}' ({roleObj?.NamaPeran}) berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            var availableRoles = _service.GetAllPeran()
                .Where(p => p.NamaPeran != "Pemilik Toko" && p.PeranId != 1)
                .Select(p => new {
                    p.PeranId,
                    NamaPeran = p.NamaPeran == "User" ? "Pelanggan" : p.NamaPeran
                })
                .ToList();
            ViewBag.PeranId = new SelectList(availableRoles, "PeranId", "NamaPeran", model.PeranId);
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var data = _service.GetPenggunaById(id);
            if (data == null) return NotFound();

            var roles = _service.GetAllPeran();
            // Jika bukan pemilik toko, jangan tampilkan opsi Pemilik Toko
            if (data.Peran?.NamaPeran != "Pemilik Toko" && data.PeranId != 1)
            {
                roles = roles.Where(p => p.NamaPeran != "Pemilik Toko" && p.PeranId != 1).ToList();
            }
            var mappedRoles = roles.Select(p => new {
                p.PeranId,
                NamaPeran = p.NamaPeran == "User" ? "Pelanggan" : p.NamaPeran
            }).ToList();
            ViewBag.PeranId = new SelectList(mappedRoles, "PeranId", "NamaPeran", data.PeranId);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Pengguna model)
        {
            var existing = _service.GetPenggunaById(model.PenggunaId);
            if (existing == null) return NotFound();

            // Cegah promosi akun lain menjadi Pemilik Toko
            if ((existing.Peran?.NamaPeran != "Pemilik Toko" && existing.PeranId != 1) &&
                (model.PeranId == 1 || _service.GetAllPeran().FirstOrDefault(p => p.PeranId == model.PeranId)?.NamaPeran == "Pemilik Toko"))
            {
                ModelState.AddModelError("PeranId", "Tidak dapat mengubah peran pengguna menjadi Pemilik Toko.");
            }

            // Validasi Nama Lengkap
            if (string.IsNullOrWhiteSpace(model.NamaLengkap) || model.NamaLengkap.Trim().Length < 3)
            {
                ModelState.AddModelError("NamaLengkap", "Nama lengkap wajib diisi minimal 3 karakter.");
            }

            // Validasi Username & Keunikan (kecuali ID sendiri)
            var (isUserValid, userErr) = ValidationHelper.ValidateUsername(model.Username);
            if (!isUserValid)
            {
                ModelState.AddModelError("Username", userErr!);
            }
            else
            {
                string cleanUser = model.Username.Trim();
                if (_service.GetAllPengguna().Any(u => u.PenggunaId != model.PenggunaId && u.DeletedAt == null && string.Equals(u.Username, cleanUser, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Username", "Username sudah digunakan oleh pengguna lain.");
                }
            }

            // Validasi Password jika diisi
            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.Remove(nameof(model.PasswordHash));
            }
            else
            {
                var (isPassValid, passErr) = ValidationHelper.ValidatePassword(model.PasswordHash, isRequired: false);
                if (!isPassValid)
                {
                    ModelState.AddModelError("PasswordHash", passErr!);
                }
            }

            // Validasi Email & Keunikan (kecuali ID sendiri)
            var (isEmailValid, emailErr) = ValidationHelper.ValidateEmail(model.Email, isRequired: true);
            if (!isEmailValid)
            {
                ModelState.AddModelError("Email", emailErr!);
            }
            else
            {
                string cleanEmail = model.Email.Trim();
                if (_service.GetAllPengguna().Any(u => u.PenggunaId != model.PenggunaId && u.DeletedAt == null && string.Equals(u.Email, cleanEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Email", "Email sudah digunakan oleh pengguna lain. Setiap akun harus memiliki email unik.");
                }
            }

            // Validasi Nomor Telepon
            var (isPhoneValid, phoneErr) = ValidationHelper.ValidateNomorTelepon(model.NomorTelepon, isRequired: true);
            if (!isPhoneValid)
            {
                ModelState.AddModelError("NomorTelepon", phoneErr!);
            }

            // Validasi Alamat
            if (string.IsNullOrWhiteSpace(model.Alamat) || model.Alamat.Trim().Length < 5)
            {
                ModelState.AddModelError("Alamat", "Alamat lengkap wajib diisi minimal 5 karakter.");
            }

            if (ModelState.IsValid)
            {
                model.NamaLengkap = model.NamaLengkap.Trim();
                model.Username = model.Username.Trim();
                model.Email = model.Email.Trim().ToLower();
                model.NomorTelepon = model.NomorTelepon?.Trim();
                model.Alamat = model.Alamat?.Trim();

                _service.UpdatePengguna(model);
                TempData["Success"] = $"Data pengguna '{model.NamaLengkap}' berhasil diperbarui.";
                return RedirectToAction(nameof(Index));
            }

            var roles = _service.GetAllPeran();
            if (existing.Peran?.NamaPeran != "Pemilik Toko" && existing.PeranId != 1)
            {
                roles = roles.Where(p => p.NamaPeran != "Pemilik Toko" && p.PeranId != 1).ToList();
            }
            var mappedRoles = roles.Select(p => new {
                p.PeranId,
                NamaPeran = p.NamaPeran == "User" ? "Pelanggan" : p.NamaPeran
            }).ToList();
            ViewBag.PeranId = new SelectList(mappedRoles, "PeranId", "NamaPeran", model.PeranId);
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
