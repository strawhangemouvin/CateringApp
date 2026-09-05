using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Context;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace CateringApp.Controllers;

public class AccountController : Controller
{
    private readonly CateringDbContext _context;
    private readonly IPasswordHasher<Pengguna> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;

    public AccountController(
        CateringDbContext context, 
        IPasswordHasher<Pengguna> passwordHasher, 
        IJwtTokenService jwtTokenService,
        IMemoryCache cache,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _cache = cache;
        _emailService = emailService;
    }

    public IActionResult Login()
    {
        var token = Request.Cookies["JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value 
                           ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                return RedirectToAction("Index", "Dashboard");
            }
            catch
            {
                Response.Cookies.Delete("JwtToken");
            }
        }
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
                .Include(u => u.Peran)
                .FirstOrDefault(u => u.Username.ToLower() == inputUsername.ToLower()
                                  && u.DeletedAt == null);

            if (user != null)
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    
                    string token = _jwtTokenService.GenerateToken(user);

                    Response.Cookies.Append("JwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    HttpContext.Session.SetInt32("UserId", user.PenggunaId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Nama", user.NamaLengkap);

                    string roleName = user.Peran?.NamaPeran ?? string.Empty;
                    string role = roleName switch
                    {
                        "Pemilik Toko" => "Pemilik Toko",
                        "Admin" => "Pemilik Toko",
                        "Karyawan" => "Karyawan",
                        "Pelanggan" => "User",
                        _ => (user.PeranId == 1 ? "Pemilik Toko" : (user.PeranId == 2 ? "Karyawan" : "User"))
                    };
                    HttpContext.Session.SetString("Role", role);

                    return RedirectToAction("Index", "Dashboard");
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
            if (_context.Penggunas.Any(u => u.Username.ToLower() == model.Username.Trim().ToLower() && u.DeletedAt == null))
            {
                ModelState.AddModelError("Username", "Username sudah digunakan.");
                return View(model);
            }

            if (_context.Penggunas.Any(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.DeletedAt == null))
            {
                ModelState.AddModelError("Email", "Email sudah terdaftar.");
                return View(model);
            }

            var customerRole = _context.Perans.FirstOrDefault(p => p.NamaPeran == "Pelanggan" || p.NamaPeran == "User");
            int customerRoleId = customerRole?.PeranId ?? 2;

            var user = new Pengguna
            {
                PeranId = customerRoleId,
                NamaLengkap = model.NamaLengkap.Trim(),
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                NomorTelepon = model.NomorTelepon.Trim(),
                Alamat = model.Alamat.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password.Trim());

            _context.Penggunas.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Pendaftaran akun berhasil! Silakan login.";
            return RedirectToAction("Login");
        }
        return View(model);
    }

    public IActionResult Logout()
    {
        Response.Cookies.Delete("JwtToken");
        HttpContext.Session.Clear();
        TempData["Success"] = "Anda telah berhasil logout.";
        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Penggunas.FirstOrDefault(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.DeletedAt == null);
            if (user != null)
            {
                
                var otpCode = new Random().Next(100000, 999999).ToString();
                var cacheKey = $"OTP_{model.Email.Trim().ToLower()}";
                
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(15));

                bool emailSent = await _emailService.SendOtpEmailAsync(model.Email.Trim(), otpCode, user.NamaLengkap);

                if (emailSent)
                {
                    TempData["Success"] = $"Kode verifikasi OTP telah dikirimkan ke email {model.Email}. Silakan periksa kotak masuk (inbox/spam) Anda.";
                }
                else
                {
                    TempData["Success"] = $"Kode verifikasi OTP Anda adalah: {otpCode}. (Catatan: Password SMTP belum diisi di appsettings.json, sistem otomatis menampilkan kode di sini).";
                    TempData["OtpDemo"] = otpCode;
                }

                return RedirectToAction("ResetPassword", new { email = model.Email.Trim() });
            }
            ModelState.AddModelError("", "Email tidak ditemukan atau akun sudah tidak aktif.");
        }
        return View(model);
    }

    public IActionResult ResetPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("ForgotPassword");
        }

        var model = new ResetPasswordViewModel { Email = email.Trim() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var cacheKey = $"OTP_{model.Email.Trim().ToLower()}";

            if (!_cache.TryGetValue(cacheKey, out string? validOtp) || validOtp != model.KodeOtp.Trim())
            {
                ModelState.AddModelError("KodeOtp", "Kode OTP salah atau sudah kadaluarsa. Silakan minta kode baru.");
                return View(model);
            }

            var user = _context.Penggunas.FirstOrDefault(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.DeletedAt == null);
            if (user != null)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password.Trim());
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                _cache.Remove(cacheKey);

                TempData["Success"] = "Password baru berhasil disimpan. Silakan login kembali.";
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Akun dengan email tersebut tidak ditemukan.");
        }
        return View(model);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
