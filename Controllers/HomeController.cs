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
            return code switch
            {
                401 => View("Error401"),
                403 => View("Error403"),
                404 => View("Error404"),
                _ => View("Error500")
            };
        }
    }
}
