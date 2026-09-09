using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CateringApp.Controllers.Api
{
    [Route("api/upload")]
    [ApiController]
    public class UploadApiController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly CateringDbContext _context;

        public UploadApiController(IWebHostEnvironment env, CateringDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "File tidak boleh kosong. Silakan pilih file gambar atau PDF."
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Ukuran file terlalu besar. Maksimal ukuran file adalah 5 MB."
                });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

            if (!allowedExtensions.Contains(extension))
            {
                return UnprocessableEntity(new
                {
                    statusCode = 422,
                    status = "fail",
                    message = $"Format file '{extension}' tidak didukung. Backend hanya mendukung file Gambar (.jpg, .jpeg, .png, .webp) atau dokumen (.pdf)."
                });
            }

            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var isPdf = extension == ".pdf";
                var prefix = isPdf ? "DOC_" : "IMG_";
                var uniqueFileName = $"{prefix}{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/{uniqueFileName}";

                return Created(fileUrl, new
                {
                    statusCode = 201,
                    status = "success",
                    message = $"File {(isPdf ? "PDF" : "Gambar")} berhasil diunggah.",
                    data = new
                    {
                        fileName = uniqueFileName,
                        originalName = file.FileName,
                        fileUrl = fileUrl,
                        fileType = isPdf ? "pdf" : "image",
                        extension = extension,
                        sizeBytes = file.Length
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal mengunggah file ke server: " + ex.Message
                });
            }
        }

        [HttpPost("bukti-bayar/{pesananId}")]
        public async Task<IActionResult> UploadBuktiBayar(int pesananId, IFormFile? file, [FromForm] string metodePembayaran = "Transfer Bank")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "File bukti transfer wajib dilampirkan (Gambar atau PDF)."
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    status = "fail",
                    message = "Ukuran file bukti pembayaran maksimal 5 MB."
                });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

            if (!allowedExtensions.Contains(extension))
            {
                return UnprocessableEntity(new
                {
                    statusCode = 422,
                    status = "fail",
                    message = $"Format file '{extension}' tidak didukung. Mohon unggah bukti pembayaran dalam format Gambar (.jpg, .jpeg, .png, .webp) atau dokumen (.pdf)."
                });
            }

            try
            {
                var pesanan = await _context.Pesanans
                    .Include(p => p.Pembayaran)
                    .FirstOrDefaultAsync(p => p.PesananId == pesananId && p.DeletedAt == null);

                if (pesanan == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        status = "fail",
                        message = $"Pesanan dengan ID {pesananId} tidak ditemukan."
                    });
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"PAY_{pesananId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/{uniqueFileName}";

                if (pesanan.Pembayaran == null)
                {
                    var newPayment = new Pembayaran
                    {
                        PesananId = pesananId,
                        MetodePembayaran = string.IsNullOrWhiteSpace(metodePembayaran) ? "Transfer Bank" : metodePembayaran.Trim(),
                        BuktiTransfer = fileUrl,
                        StatusVerifikasi = "Menunggu Verifikasi",
                        TanggalBayar = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };
                    _context.Pembayarans.Add(newPayment);
                }
                else
                {
                    pesanan.Pembayaran.BuktiTransfer = fileUrl;
                    pesanan.Pembayaran.MetodePembayaran = string.IsNullOrWhiteSpace(metodePembayaran) ? pesanan.Pembayaran.MetodePembayaran : metodePembayaran.Trim();
                    pesanan.Pembayaran.StatusVerifikasi = "Menunggu Verifikasi";
                    pesanan.Pembayaran.TanggalBayar = DateTime.Now;
                    pesanan.Pembayaran.UpdatedAt = DateTime.Now;
                }

                pesanan.StatusPesanan = "Menunggu Verifikasi";
                pesanan.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Created(fileUrl, new
                {
                    statusCode = 201,
                    status = "success",
                    message = "Bukti pembayaran berhasil diunggah. Menunggu verifikasi admin.",
                    data = new
                    {
                        pesananId = pesananId,
                        nomorPesanan = pesanan.NomorPesanan,
                        buktiUrl = fileUrl,
                        fileType = extension == ".pdf" ? "pdf" : "image",
                        statusPesanan = pesanan.StatusPesanan
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    status = "error",
                    message = "Gagal memproses unggahan bukti pembayaran: " + ex.Message
                });
            }
        }
    }
}
