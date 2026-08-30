using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Context;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace CateringApp.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly CateringDbContext _context;
        private readonly IPasswordHasher<Pengguna> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthApiController(
            CateringDbContext context,
            IPasswordHasher<Pengguna> passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usernameInput = model.Username.Trim().ToLower();
            var user = _context.Penggunas
                .Include(u => u.Peran)
                .FirstOrDefault(u => u.Username.ToLower() == usernameInput && u.DeletedAt == null);

            if (user == null)
            {
                return Unauthorized(new { message = "Username atau password tidak valid." });
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password.Trim());
            if (verificationResult != PasswordVerificationResult.Success)
            {
                return Unauthorized(new { message = "Username atau password tidak valid." });
            }

            var token = _jwtTokenService.GenerateToken(user);

            string role = user.Peran?.NamaPeran ?? user.PeranId switch
            {
                1 => "Pemilik Toko",
                2 => "Karyawan",
                _ => "User"
            };

            return Ok(new
            {
                token = token,
                userId = user.PenggunaId,
                username = user.Username,
                fullName = user.NamaLengkap,
                email = user.Email,
                role = role
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token tidak valid atau klaim tidak ditemukan." });
            }

            var user = _context.Penggunas
                .Include(u => u.Peran)
                .FirstOrDefault(u => u.PenggunaId == userId && u.DeletedAt == null);

            if (user == null)
            {
                return NotFound(new { message = "Pengguna tidak ditemukan." });
            }

            string role = user.Peran?.NamaPeran ?? user.PeranId switch
            {
                1 => "Pemilik Toko",
                2 => "Karyawan",
                _ => "User"
            };

            return Ok(new
            {
                userId = user.PenggunaId,
                username = user.Username,
                fullName = user.NamaLengkap,
                email = user.Email,
                phoneNumber = user.NomorTelepon,
                address = user.Alamat,
                role = role
            });
        }
    }
}
