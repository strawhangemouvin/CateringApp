using CateringApp.Filters;
using CateringApp.Helpers;
using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CateringApp.Controllers
{
    [SessionAuthorize]
    public class CartController : Controller
    {
        private readonly ICateringService _service;

        public CartController(ICateringService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int? userId = HttpContext.Session.GetInt32("UserId");
            string userAlamat = "";
            if (userId.HasValue)
            {
                var user = _service.GetPenggunaById(userId.Value);
                userAlamat = user?.Alamat ?? "";
            }

            decimal ongkir = OngkirHelper.HitungOngkir(userAlamat);
            string ketOngkir = OngkirHelper.GetKeteranganOngkir(userAlamat);

            ViewBag.UserAlamat = userAlamat;
            ViewBag.Ongkir = ongkir;
            ViewBag.KetOngkir = ketOngkir;

            return View(cart);
        }

        [HttpGet]
        public IActionResult CalculateOngkir(string? alamat)
        {
            var info = OngkirHelper.CekWilayah(alamat);
            return Json(new 
            { 
                isCovered = info.IsCovered,
                ongkir = info.Ongkir, 
                keterangan = info.Keterangan, 
                namaZona = info.NamaZona,
                ongkirFormat = info.IsCovered ? (info.Ongkir == 0 ? "Gratis Ongkir" : "Rp " + info.Ongkir.ToString("N0")) : "Di Luar Jangkauan" 
            });
        }

        [HttpPost]
        public IActionResult Add(int paketId, int jumlah = 10, string? catatan = null)
        {
            var paket = _service.GetPaketById(paketId);
            if (paket == null)
            {
                return NotFound();
            }

            // Batas minimal porsi katering adalah 10
            if (jumlah < 10)
            {
                jumlah = 10;
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.PaketId == paketId);
            if (cartItem != null)
            {
                cartItem.Jumlah += jumlah;
                if (!string.IsNullOrEmpty(catatan))
                {
                    cartItem.Catatan = catatan;
                }
            }
            else
            {
                string photoUrl = MenuImageHelper.GetGambarUrl(paket.NamaPaket, paket.Gambar, paket.Kategori?.NamaKategori);
                cart.Add(new CartItem
                {
                    PaketId = paket.PaketId,
                    NamaPaket = paket.NamaPaket,
                    Harga = paket.Harga,
                    Gambar = photoUrl,
                    Jumlah = jumlah,
                    Catatan = catatan
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["Success"] = $"\"{paket.NamaPaket}\" ({jumlah} porsi) berhasil ditambahkan ke keranjang.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int paketId, int jumlah)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.PaketId == paketId);
            if (item != null)
            {
                if (jumlah < 10)
                {
                    item.Jumlah = 10;
                    TempData["Error"] = "Minimal pemesanan untuk paket katering adalah 10 porsi.";
                }
                else
                {
                    item.Jumlah = jumlah;
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int? paketId, int? id)
        {
            int targetId = (paketId.HasValue && paketId.Value > 0) ? paketId.Value : (id ?? 0);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.PaketId == targetId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["Success"] = $"\"{item.NamaPaket}\" berhasil dihapus dari keranjang.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int qty)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.PaketId == id);
            if (item != null)
            {
                if (qty < 10)
                {
                    item.Jumlah = 10;
                    TempData["Error"] = "Minimal pemesanan katering adalah 10 porsi.";
                }
                else
                {
                    item.Jumlah = qty;
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCatatan(int id, string catatan)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.PaketId == id);
            if (item != null)
            {
                item.Catatan = catatan;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return Json(new { success = true });
        }

        public IActionResult Checkout()
        {
            string? role = HttpContext.Session.GetString("Role");
            if (role != "User")
            {
                TempData["Warning"] = "Fitur Keranjang Belanja & Checkout mandiri dikhususkan untuk akun Pelanggan. Untuk pesanan walk-in / WhatsApp, silakan gunakan menu 'Pesanan Manual'.";
                return RedirectToAction("Index", "Pesanan");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Keranjang belanja Anda kosong.";
                return RedirectToAction("Index");
            }

            // Validasi minimal 10 porsi per item
            if (cart.Any(c => c.Jumlah < 10))
            {
                TempData["Error"] = "Minimal pemesanan paket katering adalah 10 porsi per menu. Silakan sesuaikan jumlah porsi di keranjang.";
                return RedirectToAction("Index");
            }

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = _service.GetPenggunaById(userId);

            DateTime defaultDate = DateTime.Today.AddDays(2);
            int existingPorsi = _service.GetTotalPorsiByTanggal(defaultDate);
            int sisaKuota = Math.Max(0, 400 - existingPorsi);

            var model = new CheckoutViewModel
            {
                AlamatPengiriman = user?.Alamat ?? string.Empty,
                TanggalPengiriman = defaultDate,
                JamPengantaran = "10:00 - 12:00"
            };

            ViewBag.Cart = cart;
            ViewBag.SisaKuota = sisaKuota;
            ViewBag.TotalPorsiKeranjang = cart.Sum(c => c.Jumlah);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            string? role = HttpContext.Session.GetString("Role");
            if (role != "User")
            {
                TempData["Warning"] = "Fitur Checkout mandiri dikhususkan untuk akun Pelanggan.";
                return RedirectToAction("Index", "Pesanan");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Keranjang belanja Anda kosong.";
                return RedirectToAction("Index");
            }

            // Validasi backend: minimal 10 porsi per item
            if (cart.Any(c => c.Jumlah < 10))
            {
                TempData["Error"] = "Minimal pemesanan paket katering adalah 10 porsi per menu.";
                return RedirectToAction("Index");
            }

            // Validasi tanggal minimal H+1
            if (model.TanggalPengiriman.Date <= DateTime.Today)
            {
                ModelState.AddModelError("TanggalPengiriman", "Pemesanan katering harus dijadwalkan minimal 1 hari (H-1) sebelum tanggal acara.");
            }

            // Validasi kapasitas kuota dapur harian (Maks. 400 Porsi)
            int totalPorsiKeranjang = cart.Sum(c => c.Jumlah);
            int existingPorsi = _service.GetTotalPorsiByTanggal(model.TanggalPengiriman);
            int sisaKuota = Math.Max(0, 400 - existingPorsi);

            if (existingPorsi + totalPorsiKeranjang > 400)
            {
                ModelState.AddModelError("TanggalPengiriman", 
                    $"Mohon maaf, kuota dapur untuk tanggal {model.TanggalPengiriman:dd MMMM yyyy} tidak mencukupi (Sisa kuota: {sisaKuota} porsi, pesanan Anda: {totalPorsiKeranjang} porsi). Silakan pilih tanggal lain atau hubungi kami via WhatsApp.");
            }

            // Validasi batas jangkauan pengiriman katering (Catering Mimi Saripah - Tenajar Lor, Kertasemaya)
            var cekWilayah = OngkirHelper.CekWilayah(model.AlamatPengiriman);
            if (!cekWilayah.IsCovered)
            {
                ModelState.AddModelError("AlamatPengiriman", 
                    "Mohon maaf, lokasi pengiriman berada di luar jangkauan kurir katering Mimi Saripah (Maksimal pengantaran area Indramayu, Cirebon, Majalengka, dan Subang perbatasan). Silakan gunakan alamat dalam area operasional atau hubungi admin via WhatsApp untuk pesanan katering khusus.");
            }

            if (ModelState.IsValid)
            {
                int userId = HttpContext.Session.GetInt32("UserId")!.Value;
                int pesananId = _service.BuatPesananDariKeranjang(userId, cart, model);

                HttpContext.Session.Remove("Cart");

                TempData["Success"] = "Pesanan berhasil dibuat. Silakan lanjutkan ke pembayaran.";
                return RedirectToAction("Bayar", "Pesanan", new { id = pesananId });
            }

            ViewBag.Cart = cart;
            ViewBag.SisaKuota = sisaKuota;
            ViewBag.TotalPorsiKeranjang = totalPorsiKeranjang;
            return View(model);
        }
    }
}
