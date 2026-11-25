using Microsoft.AspNetCore.Mvc;
using SmartCampusPortal.Data;
using SmartCampusPortal.Models;

namespace SmartCampusPortal.Controllers;

public sealed class RequestsController(ServiceRequestRepository repository) : Controller
{
    public async Task<IActionResult> Index(string? search, string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && !ServiceRequestStatuses.IsValid(status))
        {
            ModelState.AddModelError(nameof(status), "Select a valid request status.");
            status = null;
        }

        return View(new ServiceRequestListViewModel
        {
            Requests = await repository.GetRequestsAsync(search, status),
            Search = search,
            Status = status
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await repository.GetByIdAsync(id);
        return request is null ? NotFound() : View(request);
    }

    [HttpGet]
    public IActionResult Create() => View(new ServiceRequestForm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestForm request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var id = await repository.CreateAsync(request);
        TempData["Success"] = "Your service request was submitted.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!ServiceRequestStatuses.IsValid(status))
        {
            return BadRequest("Select a valid request status.");
        }

        if (!await repository.UpdateStatusAsync(id, status))
        {
            return NotFound();
        }

        TempData["Success"] = "The request status was updated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
