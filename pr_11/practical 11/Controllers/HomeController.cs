using Microsoft.AspNetCore.Mvc;
using SmartCampusPortal.Data;

namespace SmartCampusPortal.Controllers;

public sealed class HomeController(ServiceRequestRepository repository) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await repository.GetDashboardAsync());
    }

    public IActionResult Error() => View();
}
