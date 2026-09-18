using System.Diagnostics;
using System.Text.Json;
using CateringApp.Helpers;
using CateringApp.Models;
using CateringApp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace CateringApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly object _fileLock = new();

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        private string GetKesanPesanFilePath()
        {
            var folder = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "kesan_pesan.json");
        }

        [HttpGet]
        public IActionResult GetKesanPesan()
        {
            try
            {
                var filePath = GetKesanPesanFilePath();
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new List<KesanPesanItem>());
                }

                string json;
                lock (_fileLock)
                {
                    json = System.IO.File.ReadAllText(filePath);
                }

                var list = JsonSerializer.Deserialize<List<KesanPesanItem>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<KesanPesanItem>();

                return Json(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membaca data kesan dan pesan.");
                return Json(new List<KesanPesanItem>());
            }
        }

        [HttpPost]
        public IActionResult TambahKesanPesan([FromBody] KesanPesanItem input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Nama) || string.IsNullOrWhiteSpace(input.Pesan))
            {
                return BadRequest(new { success = false, message = "Nama dan Pesan wajib diisi!" });
            }

            try
            {
                var sanitizedItem = new KesanPesanItem
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Nama = XssSanitizer.Sanitize(input.Nama),
                    Acara = string.IsNullOrWhiteSpace(input.Acara) ? "Pelanggan" : XssSanitizer.Sanitize(input.Acara),
                    Rating = Math.Clamp(input.Rating, 1, 5),
                    Pesan = XssSanitizer.Sanitize(input.Pesan),
                    Tanggal = DateTime.Now.ToString("dd MMM yyyy, HH:mm")
                };

                var filePath = GetKesanPesanFilePath();
                var list = new List<KesanPesanItem>();

                lock (_fileLock)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        var json = System.IO.File.ReadAllText(filePath);
                        list = JsonSerializer.Deserialize<List<KesanPesanItem>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<KesanPesanItem>();
                    }

                    list.Insert(0, sanitizedItem);

                    var updatedJson = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(filePath, updatedJson);
                }

                return Ok(new { success = true, data = sanitizedItem, message = "Terima kasih! Kesan dan pesan Anda berhasil ditampilkan." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menyimpan kesan dan pesan.");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan saat menyimpan data." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error500");
        }

        [Route("Home/ErrorStatus/{code}")]
        public IActionResult ErrorStatus(int code)
        {
            var isApi = Request.Path.StartsWithSegments("/api") || 
                        Request.Headers.Accept.ToString().Contains("application/json") ||
                        Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isApi)
            {
                string message = code switch
                {
                    400 => "Bad Request. Permintaan tidak dapat diproses karena data tidak valid.",
                    401 => "Unauthorized. Token otentikasi tidak valid atau Anda belum login.",
                    403 => "Forbidden. Anda tidak memiliki hak akses untuk mengakses resource ini.",
                    404 => "Not Found. Endpoint atau resource yang Anda tuju tidak ditemukan.",
                    422 => "Unprocessable Entity. Validasi data input gagal.",
                    _ => "Internal Server Error. Terjadi kendala teknis pada server."
                };

                return StatusCode(code, new
                {
                    statusCode = code,
                    status = code >= 500 ? "error" : "fail",
                    message = message
                });
            }

            return code switch
            {
                400 => View("Error400"),
                401 => View("Error401"),
                403 => View("Error403"),
                404 => View("Error404"),
                422 => View("Error422"),
                _ => View("Error500")
            };
        }
    }
}
