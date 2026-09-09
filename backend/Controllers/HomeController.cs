using System.Diagnostics;
using CateringApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CateringApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
