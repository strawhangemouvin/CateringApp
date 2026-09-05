using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringApp.Controllers.Api
{
    [Route("api/menu")]
    [Route("api/products")]
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public MenuApiController(CateringDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? kategoriId,
            [FromQuery] int? category,
            [FromQuery] string? status,
            [FromQuery] string? sort,
            [FromQuery] string? sortBy = "terbaru",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                
                if (page < 1) page = 1;
                if (limit < 1) limit = 10;
                if (limit > 100) limit = 100;

                var query = _context.PaketMenus
                    .Include(m => m.Kategori)
                    .Where(m => m.DeletedAt == null)
                    .AsQueryable();

                var selectedCategory = category ?? kategoriId;
                if (selectedCategory.HasValue && selectedCategory.Value > 0)
                {
                    query = query.Where(m => m.KategoriId == selectedCategory.Value);
                }

                if (!string.IsNullOrWhiteSpace(status) && status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(m => m.DeletedAt == null);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(m => m.NamaPaket.ToLower().Contains(keyword) ||
                                             (m.DeskripsiMenu != null && m.DeskripsiMenu.ToLower().Contains(keyword)));
                }

                var sortingParam = (!string.IsNullOrWhiteSpace(sort) ? sort : sortBy)?.ToLower();
                query = sortingParam switch
                {
                    "name" or "az" => query.OrderBy(m => m.NamaPaket),
                    "name_desc" or "za" => query.OrderByDescending(m => m.NamaPaket),
                    "price" or "price_asc" or "termurah" => query.OrderBy(m => m.Harga),
                    "price_desc" or "termahal" => query.OrderByDescending(m => m.Harga),
                    _ => query.OrderByDescending(m => m.CreatedAt ?? DateTime.MinValue)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)limit);

                var data = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(m => new
                    {
                        m.PaketId,
                        m.KategoriId,
                        NamaKategori = m.Kategori != null ? m.Kategori.NamaKategori : "-",
                        m.NamaPaket,
                        m.Harga,
                        m.DeskripsiMenu,
                        m.Gambar,
                        Status = m.DeletedAt == null ? "active" : "inactive",
                        m.CreatedAt,
                        m.UpdatedAt
                    }).ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Data produk/menu berhasil diambil.",
                    pagination = new
                    {
                        currentPage = page,
                        limit = limit,
                        totalItems = totalItems,
                        totalPages = totalPages,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    },
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

                var isDuplicateName = await _context.PaketMenus
                    .AnyAsync(m => m.NamaPaket.ToLower() == model.NamaPaket.Trim().ToLower() && m.DeletedAt == null);

                if (isDuplicateName)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Nama paket menu '{model.NamaPaket}' sudah digunakan. Gunakan nama lain."
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

                var isDuplicateName = await _context.PaketMenus
                    .AnyAsync(m => m.PaketId != id && m.NamaPaket.ToLower() == model.NamaPaket.Trim().ToLower() && m.DeletedAt == null);

                if (isDuplicateName)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Nama paket menu '{model.NamaPaket}' sudah digunakan oleh menu lain. Gunakan nama lain."
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
