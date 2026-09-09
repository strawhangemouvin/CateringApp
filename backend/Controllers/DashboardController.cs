using CateringApp.Filters;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CateringApp.Controllers;

[SessionAuthorize]
public class DashboardController : Controller
{
    private readonly ICateringService _service;

    public DashboardController(ICateringService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        string? role = HttpContext.Session.GetString("Role");
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (role == "User" && userId.HasValue)
        {
            var customerData = _service.GetCustomerDashboardSummary(userId.Value);
            return View("CustomerIndex", customerData);
        }
        
        var data = _service.GetDashboardSummary();
        return View(data);
    }
}
