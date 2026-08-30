using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CateringApp.Controllers;

public class AccountController : Controller
{
    private readonly CateringDbContext _context;
    private readonly IPasswordHasher<Pengguna> _passwordHasher;

    public AccountController(CateringDbContext context, IPasswordHasher<Pengguna> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            string inputUsername = model.Username.Trim();
            string inputPassword = model.Password.Trim();

            var user = _context.Penggunas
                .FirstOrDefault(u => u.Username.ToLower() == inputUsername.ToLower()
                                  && u.DeletedAt == null);

            if (user != null)
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("UserId", user.PenggunaId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Nama", user.NamaLengkap);

                    string role = user.PeranId switch
                    {
                        1 => "Pemilik Toko",
                        2 => "Karyawan",
                        _ => "User"
                    };
                    HttpContext.Session.SetString("Role", role);

                    if (role == "Pemilik Toko" || role == "Karyawan")
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Pesanan");
                    }
                }
            }
            ModelState.AddModelError("", "Username atau Password tidak valid.");
        }
        return View(model);
    }

    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_context.Penggunas.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username sudah digunakan.");
                return View(model);
            }

            var user = new Pengguna
            {
                PeranId = 3,
                NamaLengkap = model.NamaLengkap,
                Username = model.Username,
                Email = model.Email,
                NomorTelepon = model.NomorTelepon,
                Alamat = model.Alamat,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password.Trim());

            _context.Penggunas.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Anda telah berhasil logout.";
        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Penggunas.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower() && u.DeletedAt == null);
            if (user != null)
            {
                TempData["Success"] = $"Link reset ditemukan. Silakan klik link berikut untuk reset: /Account/ResetPassword?email={model.Email}";
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Email tidak terdaftar.");
        }
        return View(model);
    }

    public IActionResult ResetPassword(string email)
    {
        var model = new ResetPasswordViewModel { Email = email };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Penggunas.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower() && u.DeletedAt == null);
            if (user != null)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password.Trim());
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                TempData["Success"] = "Password berhasil diubah. Silakan login kembali.";
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "User dengan email tersebut tidak ditemukan.");
        }
        return View(model);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}