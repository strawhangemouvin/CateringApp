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
    [Route("api/pembayaran")]
    [ApiController]
    public class PembayaranApiController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public PembayaranApiController(CateringDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            try
            {
                var query = _context.Pembayarans
                    .Include(p => p.Pesanan!)
                        .ThenInclude(ps => ps.Pengguna!)
                    .Where(p => p.DeletedAt == null)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(p => p.StatusVerifikasi.ToLower() == status.Trim().ToLower());
                }

                var data = await query
                    .OrderByDescending(p => p.TanggalBayar)
                    .Select(p => new
                    {
                        p.PembayaranId,
                        p.PesananId,
                        NomorPesanan = p.Pesanan != null ? p.Pesanan.NomorPesanan : "-",
                        NamaPelanggan = p.Pesanan != null && p.Pesanan.Pengguna != null ? p.Pesanan.Pengguna.NamaLengkap : "-",
                        TotalBayar = p.Pesanan != null ? p.Pesanan.TotalBayar : 0,
                        p.MetodePembayaran,
                        p.BuktiTransfer,
                        p.StatusVerifikasi,
                        p.TanggalBayar
                    })
                    .ToListAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Daftar pembayaran berhasil diambil.",
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
                var payment = await _context.Pembayarans
                    .Include(p => p.Pesanan!)
                        .ThenInclude(ps => ps.Pengguna!)
                    .FirstOrDefaultAsync(p => p.PembayaranId == id && p.DeletedAt == null);

                if (payment == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Data pembayaran dengan ID {id} tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = "Detail pembayaran berhasil ditemukan.",
                    data = new
                    {
                        payment.PembayaranId,
                        payment.PesananId,
                        NomorPesanan = payment.Pesanan?.NomorPesanan ?? "-",
                        NamaPelanggan = payment.Pesanan?.Pengguna?.NamaLengkap ?? "-",
                        TotalBayar = payment.Pesanan?.TotalBayar ?? 0,
                        payment.MetodePembayaran,
                        payment.BuktiTransfer,
                        payment.StatusVerifikasi,
                        payment.TanggalBayar
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
        public async Task<IActionResult> Create([FromBody] CreatePembayaranDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Validasi data pembayaran gagal.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var pesanan = await _context.Pesanans
                    .Include(p => p.Pembayaran)
                    .FirstOrDefaultAsync(p => p.PesananId == model.PesananId && p.DeletedAt == null);

                if (pesanan == null)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Pesanan dengan ID {model.PesananId} tidak ditemukan."
                    });
                }

                if (pesanan.Pembayaran != null)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = $"Pesanan {pesanan.NomorPesanan} sudah memiliki data pembayaran."
                    });
                }

                if (model.TanggalBayar > DateTime.Now.AddMinutes(5))
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        status = "fail",
                        message = "Tanggal pembayaran tidak boleh melebihi waktu sekarang."
                    });
                }

                var newPembayaran = new Pembayaran
                {
                    PesananId = model.PesananId,
                    MetodePembayaran = model.MetodePembayaran.Trim(),
                    BuktiTransfer = model.BuktiTransfer.Trim(),
                    StatusVerifikasi = "Menunggu Verifikasi",
                    TanggalBayar = model.TanggalBayar,
                    CreatedAt = DateTime.Now
                };

                _context.Pembayarans.Add(newPembayaran);
                pesanan.StatusPesanan = "Menunggu Verifikasi";
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newPembayaran.PembayaranId }, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Data pembayaran berhasil dikirim.",
                    data = new
                    {
                        newPembayaran.PembayaranId,
                        newPembayaran.PesananId,
                        newPembayaran.MetodePembayaran,
                        newPembayaran.BuktiTransfer,
                        newPembayaran.StatusVerifikasi,
                        newPembayaran.TanggalBayar
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memproses pembayaran: " + ex.Message
                });
            }
        }

        [HttpPut("{id}/verifikasi")]
        public async Task<IActionResult> Verifikasi(int id, [FromBody] VerifikasiPembayaranDto model)
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
                var payment = await _context.Pembayarans
                    .Include(p => p.Pesanan)
                    .FirstOrDefaultAsync(p => p.PembayaranId == id && p.DeletedAt == null);

                if (payment == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pembayaran dengan ID {id} tidak ditemukan."
                    });
                }

                payment.StatusVerifikasi = model.StatusVerifikasi.Trim();
                payment.UpdatedAt = DateTime.Now;

                if (payment.Pesanan != null)
                {
                    if (model.StatusVerifikasi == "Valid")
                    {
                        payment.Pesanan.StatusPesanan = "Diproses";
                    }
                    else if (model.StatusVerifikasi == "Ditolak")
                    {
                        payment.Pesanan.StatusPesanan = "Menunggu Pembayaran";
                    }
                    payment.Pesanan.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    status = "success",
                    message = $"Status verifikasi pembayaran berhasil diubah menjadi '{payment.StatusVerifikasi}'.",
                    data = new
                    {
                        payment.PembayaranId,
                        payment.PesananId,
                        payment.StatusVerifikasi,
                        payment.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memverifikasi pembayaran: " + ex.Message
                });
            }
        }
    }
}
