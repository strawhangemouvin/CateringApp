using CateringApp.Helpers;
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
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        var token = Request.Cookies["JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var jwtSettings = configuration.GetSection("Jwt");
                var key = jwtSettings["Key"] ?? "CateringAppSuperSecretKeyForJwtAuthenticationServiceNet8";
                var issuer = jwtSettings["Issuer"] ?? "CateringApp";
                var audience = jwtSettings["Audience"] ?? "CateringAppUsers";

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
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
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, inputPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    
                    string token = _jwtTokenService.GenerateToken(user);

                    Response.Cookies.Append("JwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(4)
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

    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32("UserId") != null || !string.IsNullOrEmpty(Request.Cookies["JwtToken"]))
        {
            TempData["Info"] = "Anda sudah masuk ke sistem.";
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var (isUserValid, userErr) = ValidationHelper.ValidateUsername(model.Username);
            if (!isUserValid)
            {
                ModelState.AddModelError("Username", userErr!);
                return View(model);
            }

            var (isPassValid, passErr) = ValidationHelper.ValidatePassword(model.Password, isRequired: true);
            if (!isPassValid)
            {
                ModelState.AddModelError("Password", passErr!);
                return View(model);
            }

            var (isPhoneValid, phoneErr) = ValidationHelper.ValidateNomorTelepon(model.NomorTelepon, isRequired: true);
            if (!isPhoneValid)
            {
                ModelState.AddModelError("NomorTelepon", phoneErr!);
                return View(model);
            }

            var (isEmailValid, emailErr) = ValidationHelper.ValidateEmail(model.Email, isRequired: true);
            if (!isEmailValid)
            {
                ModelState.AddModelError("Email", emailErr!);
                return View(model);
            }

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

            TempData["Success"] = "Pendaftaran akun berhasil! Silakan masuk.";
            return RedirectToAction("Login");
        }
        return View(model);
    }

    public IActionResult Logout()
    {
        Response.Cookies.Delete("JwtToken");
        HttpContext.Session.Clear();
        TempData["Success"] = "Anda telah berhasil keluar dari sesi.";
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

                TempData["Success"] = "Kata sandi baru berhasil disimpan. Silakan masuk kembali.";
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Akun dengan email tersebut tidak ditemukan.");
        }
        return View(model);
    }

    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("Error403");
    }
}
