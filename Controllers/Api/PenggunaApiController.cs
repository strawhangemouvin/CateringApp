using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CateringApp.Controllers.Api
{
    [Route("api/pengguna")]
    [ApiController]
    public class PenggunaApiController : ControllerBase
    {
        private readonly CateringDbContext _context;
        private readonly IPasswordHasher<Pengguna> _passwordHasher;

        public PenggunaApiController(CateringDbContext context, IPasswordHasher<Pengguna> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? roleId)
        {
            try
            {
                var query = _context.Penggunas
                    .Include(u => u.Peran)
                    .Where(u => u.DeletedAt == null)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(keyword) ||
                                             u.NamaLengkap.ToLower().Contains(keyword) ||
                                             u.Email.ToLower().Contains(keyword));
                }

                if (roleId.HasValue && roleId.Value > 0)
                {
                    query = query.Where(u => u.PeranId == roleId.Value);
                }

                var data = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new
                    {
                        u.PenggunaId,
                        u.PeranId,
                        NamaPeran = u.Peran != null ? u.Peran.NamaPeran : "-",
                        u.NamaLengkap,
                        u.Username,
                        u.Email,
                        u.NomorTelepon,
                        u.Alamat,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Daftar pengguna berhasil diambil.",
                    total = data.Count,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Terjadi kesalahan internal pada server: " + ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _context.Penggunas
                    .Include(u => u.Peran)
                    .FirstOrDefaultAsync(u => u.PenggunaId == id && u.DeletedAt == null);

                if (user == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pengguna dengan ID {id} tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Detail pengguna berhasil ditemukan.",
                    data = new
                    {
                        user.PenggunaId,
                        user.PeranId,
                        NamaPeran = user.Peran?.NamaPeran ?? "-",
                        user.NamaLengkap,
                        user.Username,
                        user.Email,
                        user.NomorTelepon,
                        user.Alamat,
                        user.CreatedAt,
                        user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Terjadi kesalahan internal pada server: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Validasi data pengguna gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                
                var usernameExists = await _context.Penggunas
                    .AnyAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower() && u.DeletedAt == null);

                if (usernameExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Username '{model.Username}' sudah digunakan. Silakan pilih username lain."
                    });
                }

                var emailExists = await _context.Penggunas
                    .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.DeletedAt == null);

                if (emailExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Email '{model.Email}' sudah terdaftar. Silakan gunakan email lain."
                    });
                }

                var roleExists = await _context.Perans.AnyAsync(p => p.PeranId == model.PeranId);
                if (!roleExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Peran ID '{model.PeranId}' tidak valid. Pilihan: 1 (Admin/Pemilik Toko), 2 (Pelanggan)."
                    });
                }

                var newUser = new Pengguna
                {
                    PeranId = model.PeranId,
                    NamaLengkap = model.NamaLengkap.Trim(),
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    NomorTelepon = model.NomorTelepon.Trim(),
                    Alamat = model.Alamat.Trim(),
                    CreatedAt = DateTime.Now
                };

                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password.Trim());

                _context.Penggunas.Add(newUser);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newUser.PenggunaId }, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Pengguna baru berhasil ditambahkan.",
                    data = new
                    {
                        newUser.PenggunaId,
                        newUser.PeranId,
                        newUser.NamaLengkap,
                        newUser.Username,
                        newUser.Email,
                        newUser.NomorTelepon,
                        newUser.Alamat,
                        newUser.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal menambahkan pengguna: " + ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Validasi data pengguna gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var existingUser = await _context.Penggunas
                    .FirstOrDefaultAsync(u => u.PenggunaId == id && u.DeletedAt == null);

                if (existingUser == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pengguna dengan ID {id} tidak ditemukan."
                    });
                }

                var emailExists = await _context.Penggunas
                    .AnyAsync(u => u.PenggunaId != id && u.Email.ToLower() == model.Email.Trim().ToLower() && u.DeletedAt == null);

                if (emailExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Email '{model.Email}' sudah digunakan oleh pengguna lain."
                    });
                }

                var roleExists = await _context.Perans.AnyAsync(p => p.PeranId == model.PeranId);
                if (!roleExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Peran ID '{model.PeranId}' tidak valid. Pilihan: 1 (Admin/Pemilik Toko), 2 (Pelanggan)."
                    });
                }

                existingUser.PeranId = model.PeranId;
                existingUser.NamaLengkap = model.NamaLengkap.Trim();
                existingUser.Email = model.Email.Trim();
                existingUser.NomorTelepon = model.NomorTelepon.Trim();
                existingUser.Alamat = model.Alamat.Trim();
                existingUser.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Data pengguna berhasil diperbarui.",
                    data = new
                    {
                        existingUser.PenggunaId,
                        existingUser.PeranId,
                        existingUser.NamaLengkap,
                        existingUser.Username,
                        existingUser.Email,
                        existingUser.NomorTelepon,
                        existingUser.Alamat,
                        existingUser.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui pengguna: " + ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Penggunas
                    .FirstOrDefaultAsync(u => u.PenggunaId == id && u.DeletedAt == null);

                if (user == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pengguna dengan ID {id} tidak ditemukan."
                    });
                }

                user.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = $"Pengguna '{user.Username}' berhasil dihapus (soft delete)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal menghapus pengguna: " + ex.Message
                });
            }
        }
    }
}
