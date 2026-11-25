using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers;

public sealed class InventoryController(InventoryRepository repository) : Controller
{
    public IActionResult Error() => View();

    public async Task<IActionResult> Index(string? search)
    {
        return View(await repository.GetDashboardAsync(search));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await repository.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpGet]
    public IActionResult Create() => View(new ProductFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormModel product)
    {
        if (await repository.SkuExistsAsync(product.Sku))
        {
            ModelState.AddModelError(nameof(product.Sku), "That SKU is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var id = await repository.CreateAsync(product);
        TempData["Success"] = $"{product.Name} was added to inventory.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.ProductId = id;
        return View(ProductFormModel.From(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormModel product)
    {
        if (await repository.SkuExistsAsync(product.Sku, id))
        {
            ModelState.AddModelError(nameof(product.Sku), "That SKU is already in use.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            return View(product);
        }

        if (!await repository.UpdateAsync(id, product))
        {
            return NotFound();
        }

        TempData["Success"] = $"{product.Name} was updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await repository.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await repository.DeleteAsync(id))
        {
            return NotFound();
        }

        TempData["Success"] = "The product was removed from inventory.";
        return RedirectToAction(nameof(Index));
    }
}
