using CateringApp.Models.DTO;
using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // 1. GET: api/pesanan
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            try
            {
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

                var data = await query
                    .OrderByDescending(p => p.TanggalPesan)
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

        // 2. GET: api/pesanan/{id}
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

        // 3. PATCH: api/pesanan/{id}/status (Partial Update Status Pesanan)
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

                var allowedStatuses = new[] { "Pending", "Diproses", "Dikirim", "Selesai", "Dibatalkan" };
                if (!allowedStatuses.Any(s => s.Equals(model.StatusPesanan.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Status '{model.StatusPesanan}' tidak valid. Pilihan yang valid: {string.Join(", ", allowedStatuses)}"
                    });
                }

                pesanan.StatusPesanan = model.StatusPesanan.Trim();
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
