using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringApp.Controllers.Api
{
    [Route("api/menu")]
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public MenuApiController(CateringDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/menu
        // Supports filtering by keyword, category, and sorting
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? kategoriId,
            [FromQuery] string? sortBy = "terbaru")
        {
            try
            {
                var query = _context.PaketMenus
                    .Include(m => m.Kategori)
                    .Where(m => m.DeletedAt == null)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(m => m.NamaPaket.ToLower().Contains(keyword) ||
                                             (m.DeskripsiMenu != null && m.DeskripsiMenu.ToLower().Contains(keyword)));
                }

                if (kategoriId.HasValue && kategoriId.Value > 0)
                {
                    query = query.Where(m => m.KategoriId == kategoriId.Value);
                }

                query = sortBy?.ToLower() switch
                {
                    "termurah" => query.OrderBy(m => m.Harga),
                    "termahal" => query.OrderByDescending(m => m.Harga),
                    "az" => query.OrderBy(m => m.NamaPaket),
                    "za" => query.OrderByDescending(m => m.NamaPaket),
                    _ => query.OrderByDescending(m => m.CreatedAt ?? DateTime.MinValue)
                };

                var data = await query.Select(m => new
                {
                    m.PaketId,
                    m.KategoriId,
                    NamaKategori = m.Kategori != null ? m.Kategori.NamaKategori : "-",
                    m.NamaPaket,
                    m.Harga,
                    m.DeskripsiMenu,
                    m.Gambar,
                    m.CreatedAt,
                    m.UpdatedAt
                }).ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Data menu berhasil diambil.",
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

        // 2. GET: api/menu/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var menu = await _context.PaketMenus
                    .Include(m => m.Kategori)
                    .FirstOrDefaultAsync(m => m.PaketId == id && m.DeletedAt == null);

                if (menu == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Menu dengan ID {id} tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Detail menu berhasil ditemukan.",
                    data = new
                    {
                        menu.PaketId,
                        menu.KategoriId,
                        NamaKategori = menu.Kategori?.NamaKategori ?? "-",
                        menu.NamaPaket,
                        menu.Harga,
                        menu.DeskripsiMenu,
                        menu.Gambar,
                        menu.CreatedAt,
                        menu.UpdatedAt
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

        // 3. POST: api/menu
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMenuDto model)
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
                var kategoriExists = await _context.KategoriMenus
                    .AnyAsync(k => k.KategoriId == model.KategoriId && k.DeletedAt == null);

                if (!kategoriExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Kategori yang dipilih tidak valid atau sudah dihapus."
                    });
                }

                var newMenu = new PaketMenu
                {
                    KategoriId = model.KategoriId,
                    NamaPaket = model.NamaPaket.Trim(),
                    Harga = model.Harga,
                    DeskripsiMenu = model.DeskripsiMenu?.Trim(),
                    Gambar = string.IsNullOrWhiteSpace(model.Gambar) ? "/images/default-food.jpg" : model.Gambar.Trim(),
                    CreatedAt = DateTime.Now
                };

                _context.PaketMenus.Add(newMenu);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newMenu.PaketId }, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Paket menu baru berhasil dibuat.",
                    data = new
                    {
                        newMenu.PaketId,
                        newMenu.KategoriId,
                        newMenu.NamaPaket,
                        newMenu.Harga,
                        newMenu.DeskripsiMenu,
                        newMenu.Gambar,
                        newMenu.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal membuat menu: " + ex.Message
                });
            }
        }

        // 4. PUT: api/menu/{id} (Full Update)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuDto model)
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
                var existingMenu = await _context.PaketMenus
                    .FirstOrDefaultAsync(m => m.PaketId == id && m.DeletedAt == null);

                if (existingMenu == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Menu dengan ID {id} tidak ditemukan."
                    });
                }

                var kategoriExists = await _context.KategoriMenus
                    .AnyAsync(k => k.KategoriId == model.KategoriId && k.DeletedAt == null);

                if (!kategoriExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Kategori yang dipilih tidak valid."
                    });
                }

                existingMenu.KategoriId = model.KategoriId;
                existingMenu.NamaPaket = model.NamaPaket.Trim();
                existingMenu.Harga = model.Harga;
                existingMenu.DeskripsiMenu = model.DeskripsiMenu?.Trim();
                if (!string.IsNullOrWhiteSpace(model.Gambar))
                {
                    existingMenu.Gambar = model.Gambar.Trim();
                }
                existingMenu.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Seluruh data menu berhasil diperbarui.",
                    data = existingMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui menu: " + ex.Message
                });
            }
        }

        // 5. PATCH: api/menu/{id} (Partial Update)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] PatchMenuDto model)
        {
            try
            {
                var existingMenu = await _context.PaketMenus
                    .FirstOrDefaultAsync(m => m.PaketId == id && m.DeletedAt == null);

                if (existingMenu == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Menu dengan ID {id} tidak ditemukan."
                    });
                }

                bool isModified = false;

                if (model.KategoriId.HasValue)
                {
                    var kategoriExists = await _context.KategoriMenus
                        .AnyAsync(k => k.KategoriId == model.KategoriId.Value && k.DeletedAt == null);

                    if (!kategoriExists)
                    {
                        return BadRequest(new { statusCode = 400, status = "fail", message = "Kategori tidak valid." });
                    }
                    existingMenu.KategoriId = model.KategoriId.Value;
                    isModified = true;
                }

                if (!string.IsNullOrWhiteSpace(model.NamaPaket))
                {
                    existingMenu.NamaPaket = model.NamaPaket.Trim();
                    isModified = true;
                }

                if (model.Harga.HasValue)
                {
                    if (model.Harga.Value < 1000)
                    {
                        return BadRequest(new { statusCode = 400, status = "fail", message = "Harga minimal Rp 1.000." });
                    }
                    existingMenu.Harga = model.Harga.Value;
                    isModified = true;
                }

                if (model.DeskripsiMenu != null)
                {
                    existingMenu.DeskripsiMenu = model.DeskripsiMenu.Trim();
                    isModified = true;
                }

                if (!string.IsNullOrWhiteSpace(model.Gambar))
                {
                    existingMenu.Gambar = model.Gambar.Trim();
                    isModified = true;
                }

                if (isModified)
                {
                    existingMenu.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Sebagian data menu berhasil diperbarui.",
                    data = existingMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui menu: " + ex.Message
                });
            }
        }

        // 6. DELETE: api/menu/{id} (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var menu = await _context.PaketMenus
                    .FirstOrDefaultAsync(m => m.PaketId == id && m.DeletedAt == null);

                if (menu == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Menu dengan ID {id} tidak ditemukan atau sudah dihapus."
                    });
                }

                menu.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = $"Menu '{menu.NamaPaket}' berhasil dihapus."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal menghapus menu: " + ex.Message
                });
            }
        }
    }
}
