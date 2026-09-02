using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringApp.Controllers.Api
{
    [Route("api/kategori")]
    [ApiController]
    public class KategoriApiController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public KategoriApiController(CateringDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/kategori
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.KategoriMenus
                    .Where(k => k.DeletedAt == null)
                    .OrderBy(k => k.NamaKategori)
                    .Select(k => new
                    {
                        k.KategoriId,
                        k.NamaKategori,
                        k.Deskripsi,
                        k.CreatedAt,
                        k.UpdatedAt,
                        TotalMenu = _context.PaketMenus.Count(m => m.KategoriId == k.KategoriId && m.DeletedAt == null)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Daftar kategori berhasil diambil.",
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

        // 2. GET: api/kategori/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var kategori = await _context.KategoriMenus
                    .FirstOrDefaultAsync(k => k.KategoriId == id && k.DeletedAt == null);

                if (kategori == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Kategori dengan ID {id} tidak ditemukan."
                    });
                }

                var totalMenu = await _context.PaketMenus
                    .CountAsync(m => m.KategoriId == id && m.DeletedAt == null);

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Detail kategori berhasil ditemukan.",
                    data = new
                    {
                        kategori.KategoriId,
                        kategori.NamaKategori,
                        kategori.Deskripsi,
                        kategori.CreatedAt,
                        kategori.UpdatedAt,
                        totalMenu
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

        // 3. POST: api/kategori
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKategoriDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var isDuplicate = await _context.KategoriMenus
                    .AnyAsync(k => k.NamaKategori.ToLower() == model.NamaKategori.Trim().ToLower() && k.DeletedAt == null);

                if (isDuplicate)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Kategori dengan nama '{model.NamaKategori}' sudah ada."
                    });
                }

                var newKategori = new KategoriMenu
                {
                    NamaKategori = model.NamaKategori.Trim(),
                    Deskripsi = model.Deskripsi?.Trim(),
                    CreatedAt = DateTime.Now
                };

                _context.KategoriMenus.Add(newKategori);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newKategori.KategoriId }, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Kategori menu baru berhasil dibuat.",
                    data = newKategori
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal membuat kategori: " + ex.Message
                });
            }
        }

        // 4. PUT: api/kategori/{id} (Full Update)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKategoriDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var existingKategori = await _context.KategoriMenus
                    .FirstOrDefaultAsync(k => k.KategoriId == id && k.DeletedAt == null);

                if (existingKategori == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Kategori dengan ID {id} tidak ditemukan."
                    });
                }

                var isDuplicate = await _context.KategoriMenus
                    .AnyAsync(k => k.KategoriId != id && k.NamaKategori.ToLower() == model.NamaKategori.Trim().ToLower() && k.DeletedAt == null);

                if (isDuplicate)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Kategori dengan nama '{model.NamaKategori}' sudah ada."
                    });
                }

                existingKategori.NamaKategori = model.NamaKategori.Trim();
                existingKategori.Deskripsi = model.Deskripsi?.Trim();
                existingKategori.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Kategori berhasil diperbarui secara keseluruhan.",
                    data = existingKategori
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui kategori: " + ex.Message
                });
            }
        }

        // 5. PATCH: api/kategori/{id} (Partial Update)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] PatchKategoriDto model)
        {
            try
            {
                var existingKategori = await _context.KategoriMenus
                    .FirstOrDefaultAsync(k => k.KategoriId == id && k.DeletedAt == null);

                if (existingKategori == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Kategori dengan ID {id} tidak ditemukan."
                    });
                }

                bool isModified = false;

                if (!string.IsNullOrWhiteSpace(model.NamaKategori))
                {
                    var isDuplicate = await _context.KategoriMenus
                        .AnyAsync(k => k.KategoriId != id && k.NamaKategori.ToLower() == model.NamaKategori.Trim().ToLower() && k.DeletedAt == null);

                    if (isDuplicate)
                    {
                        return BadRequest(new
                        {
                            statusCode = 400,
                            status = "fail",
                            message = $"Kategori dengan nama '{model.NamaKategori}' sudah ada."
                        });
                    }

                    existingKategori.NamaKategori = model.NamaKategori.Trim();
                    isModified = true;
                }

                if (model.Deskripsi != null)
                {
                    existingKategori.Deskripsi = model.Deskripsi.Trim();
                    isModified = true;
                }

                if (isModified)
                {
                    existingKategori.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Sebagian data kategori berhasil diperbarui.",
                    data = existingKategori
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui kategori: " + ex.Message
                });
            }
        }

        // 6. DELETE: api/kategori/{id} (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var kategori = await _context.KategoriMenus
                    .FirstOrDefaultAsync(k => k.KategoriId == id && k.DeletedAt == null);

                if (kategori == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Kategori dengan ID {id} tidak ditemukan atau sudah dihapus."
                    });
                }

                // Check if category is used in active menus
                var menuCount = await _context.PaketMenus
                    .CountAsync(m => m.KategoriId == id && m.DeletedAt == null);

                if (menuCount > 0)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Kategori '{kategori.NamaKategori}' tidak dapat dihapus karena masih digunakan oleh {menuCount} paket menu aktif."
                    });
                }

                kategori.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = $"Kategori '{kategori.NamaKategori}' berhasil dihapus."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal menghapus kategori: " + ex.Message
                });
            }
        }
    }
}
