using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CateringApp.Controllers.Api
{
    [Route("api/pesanan")]
    [ApiController]
    public class PesananApiController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public PesananApiController(CateringDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (limit < 1) limit = 10;
                if (limit > 100) limit = 100;

                var query = _context.Pesanans
                    .Include(p => p.Pengguna)
                    .Include(p => p.Pembayaran)
                    .Include(p => p.DetailPesanans)
                        .ThenInclude(d => d.Paket)
                    .Where(p => p.DeletedAt == null)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(p => p.NomorPesanan.ToLower().Contains(keyword) ||
                                             (p.Pengguna != null && p.Pengguna.NamaLengkap.ToLower().Contains(keyword)));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(p => p.StatusPesanan.ToLower() == status.Trim().ToLower());
                }

                query = sort?.ToLower() switch
                {
                    "date_asc" or "terlama" => query.OrderBy(p => p.TanggalPesan),
                    "total_asc" => query.OrderBy(p => p.TotalBayar),
                    "total_desc" => query.OrderByDescending(p => p.TotalBayar),
                    _ => query.OrderByDescending(p => p.TanggalPesan)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)limit);

                var data = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.PesananId,
                        p.NomorPesanan,
                        p.PenggunaId,
                        NamaPemesan = p.Pengguna != null ? p.Pengguna.NamaLengkap : "-",
                        p.TanggalPesan,
                        p.TanggalPengiriman,
                        p.AlamatPengiriman,
                        p.TotalBayar,
                        p.StatusPesanan,
                        StatusPembayaran = p.Pembayaran != null ? p.Pembayaran.StatusVerifikasi : "Belum Bayar",
                        ItemCount = p.DetailPesanans.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Daftar pesanan berhasil diambil.",
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
                var pesanan = await _context.Pesanans
                    .Include(p => p.Pengguna)
                    .Include(p => p.Pembayaran)
                    .Include(p => p.DetailPesanans)
                        .ThenInclude(d => d.Paket)
                    .FirstOrDefaultAsync(p => p.PesananId == id && p.DeletedAt == null);

                if (pesanan == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pesanan dengan ID {id} tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Detail pesanan berhasil ditemukan.",
                    data = new
                    {
                        pesanan.PesananId,
                        pesanan.NomorPesanan,
                        pesanan.PenggunaId,
                        NamaPemesan = pesanan.Pengguna?.NamaLengkap ?? "-",
                        EmailPemesan = pesanan.Pengguna?.Email ?? "-",
                        TeleponPemesan = pesanan.Pengguna?.NomorTelepon ?? "-",
                        pesanan.TanggalPesan,
                        pesanan.TanggalPengiriman,
                        pesanan.AlamatPengiriman,
                        pesanan.TotalBayar,
                        pesanan.StatusPesanan,
                        Pembayaran = pesanan.Pembayaran == null ? null : new
                        {
                            pesanan.Pembayaran.PembayaranId,
                            pesanan.Pembayaran.MetodePembayaran,
                            pesanan.Pembayaran.BuktiTransfer,
                            pesanan.Pembayaran.StatusVerifikasi,
                            pesanan.Pembayaran.TanggalBayar
                        },
                        Items = pesanan.DetailPesanans.Select(d => new
                        {
                            d.DetailId,
                            d.PaketId,
                            NamaPaket = d.Paket?.NamaPaket ?? "-",
                            Gambar = d.Paket?.Gambar,
                            d.Jumlah,
                            d.HargaSatuan,
                            d.Subtotal,
                            d.Catatan
                        })
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
        public async Task<IActionResult> Create([FromBody] CreateOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Validasi pesanan gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                
                if (model.TanggalPengiriman.Date < DateTime.Today)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Tanggal pengiriman tidak boleh di masa lalu (minimal hari ini atau setelahnya)."
                    });
                }

                var userExists = await _context.Penggunas.AnyAsync(u => u.PenggunaId == model.PenggunaId && u.DeletedAt == null);
                if (!userExists)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Pengguna dengan ID {model.PenggunaId} tidak ditemukan."
                    });
                }

                decimal totalBayar = 0;
                var detailItems = new List<DetailPesanan>();

                foreach (var item in model.Items)
                {
                    var menu = await _context.PaketMenus.FirstOrDefaultAsync(m => m.PaketId == item.PaketId && m.DeletedAt == null);
                    if (menu == null)
                    {
                        return BadRequest(new
                        {
                            statusCode = 400,
                            status = "fail",
                            message = $"Paket Menu dengan ID {item.PaketId} tidak ditemukan atau sudah tidak aktif."
                        });
                    }

                    var subtotal = menu.Harga * item.Jumlah;
                    totalBayar += subtotal;

                    detailItems.Add(new DetailPesanan
                    {
                        PaketId = item.PaketId,
                        Jumlah = item.Jumlah,
                        HargaSatuan = menu.Harga,
                        Subtotal = subtotal,
                        Catatan = item.Catatan?.Trim()
                    });
                }

                var nomorPesanan = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(100, 999);

                var newOrder = new Pesanan
                {
                    NomorPesanan = nomorPesanan,
                    PenggunaId = model.PenggunaId,
                    TanggalPesan = DateTime.Now,
                    TanggalPengiriman = model.TanggalPengiriman,
                    AlamatPengiriman = model.AlamatPengiriman.Trim(),
                    TotalBayar = totalBayar,
                    StatusPesanan = "Pending",
                    CreatedAt = DateTime.Now,
                    DetailPesanans = detailItems
                };

                _context.Pesanans.Add(newOrder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newOrder.PesananId }, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Pesanan baru berhasil dibuat.",
                    data = new
                    {
                        newOrder.PesananId,
                        newOrder.NomorPesanan,
                        newOrder.PenggunaId,
                        newOrder.TanggalPesan,
                        newOrder.TanggalPengiriman,
                        newOrder.AlamatPengiriman,
                        newOrder.TotalBayar,
                        newOrder.StatusPesanan,
                        ItemCount = newOrder.DetailPesanans.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal membuat pesanan: " + ex.Message
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto model)
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
                var pesanan = await _context.Pesanans
                    .Include(p => p.Pembayaran)
                    .FirstOrDefaultAsync(p => p.PesananId == id && p.DeletedAt == null);

                if (pesanan == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pesanan dengan ID {id} tidak ditemukan."
                    });
                }

                var allowedStatuses = new[] { "Pending", "Menunggu Pembayaran", "Menunggu Verifikasi", "Diproses", "Dikirim", "Selesai", "Dibatalkan" };
                if (!allowedStatuses.Any(s => s.Equals(model.StatusPesanan.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Status '{model.StatusPesanan}' tidak valid. Pilihan enum yang valid: {string.Join(", ", allowedStatuses)}"
                    });
                }

                var current = pesanan.StatusPesanan;
                var targetStatus = model.StatusPesanan.Trim();

                if (current == "Selesai")
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Pesanan ini sudah SELESAI dan barang telah diterima. Status tidak dapat diubah lagi."
                    });
                }

                if (current == "Dibatalkan" || current == "Batal")
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Pesanan ini sudah DIBATALKAN. Status tidak dapat diubah lagi."
                    });
                }

                if (targetStatus == "Pending")
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Tidak dapat mengembalikan status pesanan yang sedang berjalan kembali ke 'Pending'."
                    });
                }

                var requiresPaymentStatuses = new[] { "Diproses", "Dikirim", "Selesai" };
                if (requiresPaymentStatuses.Any(s => s.Equals(targetStatus, StringComparison.OrdinalIgnoreCase)))
                {
                    if (pesanan.Pembayaran == null || pesanan.Pembayaran.StatusVerifikasi != "Valid")
                    {
                        return BadRequest(new
                        {
                            statusCode = 400,
                            status = "fail",
                            message = $"Pesanan tidak dapat diubah ke status '{targetStatus}' karena pembayaran belum diverifikasi (Valid)."
                        });
                    }
                }

                if (targetStatus == "Dikirim" && current != "Diproses")
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Pesanan harus dalam status 'Diproses' terlebih dahulu sebelum dapat diubah menjadi 'Dikirim'."
                    });
                }

                if (targetStatus == "Selesai" && current != "Dikirim")
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Pesanan harus dalam status 'Dikirim' terlebih dahulu sebelum dapat diselesaikan (Selesai)."
                    });
                }

                pesanan.StatusPesanan = targetStatus;
                pesanan.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = $"Status pesanan {pesanan.NomorPesanan} berhasil diperbarui menjadi '{pesanan.StatusPesanan}'.",
                    data = new
                    {
                        pesanan.PesananId,
                        pesanan.NomorPesanan,
                        pesanan.StatusPesanan,
                        pesanan.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memperbarui status pesanan: " + ex.Message
                });
            }
        }
    }
}
