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
            return View(cart);
        }

        [HttpPost]
        public IActionResult Add(int paketId, int jumlah = 1, string? catatan = null)
        {
            var paket = _service.GetPaketById(paketId);
            if (paket == null)
            {
                return NotFound();
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
                cart.Add(new CartItem
                {
                    PaketId = paket.PaketId,
                    NamaPaket = paket.NamaPaket,
                    Harga = paket.Harga,
                    Gambar = paket.Gambar ?? "/images/menu_box.svg",
                    Jumlah = jumlah,
                    Catatan = catatan
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["Success"] = $"\"{paket.NamaPaket}\" berhasil ditambahkan ke keranjang.";
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.PaketId == id);
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
                if (qty <= 0)
                {
                    cart.Remove(item);
                    TempData["Success"] = $"\"{item.NamaPaket}\" berhasil dihapus dari keranjang.";
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
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Keranjang belanja Anda kosong.";
                return RedirectToAction("Index");
            }

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = _service.GetPenggunaById(userId);

            var model = new CheckoutViewModel
            {
                AlamatPengiriman = user?.Alamat ?? string.Empty,
                TanggalPengiriman = DateTime.Today.AddDays(2) 
            };

            ViewBag.Cart = cart;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Keranjang belanja Anda kosong.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                int userId = HttpContext.Session.GetInt32("UserId")!.Value;
                int pesananId = _service.BuatPesananDariKeranjang(userId, cart, model);

                HttpContext.Session.Remove("Cart");

                TempData["Success"] = "Pesanan berhasil dibuat. Silakan unggah bukti pembayaran.";
                return RedirectToAction("Bayar", "Pesanan", new { id = pesananId });
            }

            ViewBag.Cart = cart;
            return View(model);
        }
    }
}
