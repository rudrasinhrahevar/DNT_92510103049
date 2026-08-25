using System.Diagnostics;
using EventRegistrationPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationPortal.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new EventRegistration();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(EventRegistration model)
    {
        // Normalize checkbox post values so a checked box is always treated as true.
        if (Request.Form.TryGetValue(nameof(EventRegistration.TermsAccepted), out var postedTerms))
        {
            model.TermsAccepted = postedTerms.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
        }

        ModelState.Remove(nameof(EventRegistration.TermsAccepted));
        if (!model.TermsAccepted)
        {
            ModelState.AddModelError(nameof(EventRegistration.TermsAccepted), "You must agree to the terms and conditions.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Registration successful! Thank you, {model.FullName}. Your dancing event tickets have been confirmed.";
        ModelState.Clear();

        return View(new EventRegistration());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
