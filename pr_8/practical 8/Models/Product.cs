using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models;

public sealed class Product
{
    public int Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int ReorderLevel { get; init; }
    public decimal UnitPrice { get; init; }
    public string Supplier { get; init; } = string.Empty;

    public bool IsLowStock => Quantity <= ReorderLevel;
}

public sealed class ProductFormModel
{
    [Required, StringLength(30)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public int Quantity { get; set; }

    [Range(0, 1_000_000)]
    [Display(Name = "Reorder level")]
    public int ReorderLevel { get; set; }

    [Range(typeof(decimal), "0.01", "10000000")]
    [DataType(DataType.Currency)]
    [Display(Name = "Unit price")]
    public decimal UnitPrice { get; set; }

    [StringLength(100)]
    public string Supplier { get; set; } = string.Empty;

    public static ProductFormModel From(Product product) => new()
    {
        Sku = product.Sku,
        Name = product.Name,
        Category = product.Category,
        Quantity = product.Quantity,
        ReorderLevel = product.ReorderLevel,
        UnitPrice = product.UnitPrice,
        Supplier = product.Supplier
    };
}
