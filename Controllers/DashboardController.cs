using CateringApp.Filters;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CateringApp.Controllers;

[SessionAuthorize("Pemilik Toko", "Karyawan")]
public class DashboardController : Controller
{
    private readonly ICateringService _service;

    public DashboardController(ICateringService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        var data = _service.GetDashboardSummary();
        return View(data);
    }
}

