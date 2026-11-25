namespace InventoryManagement.Models;

public sealed class InventoryDashboardViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public int TotalProducts { get; init; }
    public int TotalUnits { get; init; }
    public decimal InventoryValue { get; init; }
    public int LowStockCount { get; init; }
    public string? Search { get; init; }
}
