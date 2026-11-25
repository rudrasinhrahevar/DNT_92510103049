using InventoryManagement.Models;
using Microsoft.Data.Sqlite;

namespace InventoryManagement.Data;

public sealed class InventoryRepository
{
    private readonly string connectionString;

    public InventoryRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException("The InventoryDatabase connection string is missing.");
        var connection = new SqliteConnectionStringBuilder(configuredConnectionString);

        if (connection.DataSource != ":memory:" && !Path.IsPathRooted(connection.DataSource))
        {
            connection.DataSource = Path.Combine(environment.ContentRootPath, connection.DataSource);
        }

        var databaseDirectory = Path.GetDirectoryName(connection.DataSource);
        if (!string.IsNullOrEmpty(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        connectionString = connection.ToString();
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Sku TEXT NOT NULL COLLATE NOCASE UNIQUE,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
                    ReorderLevel INTEGER NOT NULL CHECK (ReorderLevel >= 0),
                    UnitPrice REAL NOT NULL CHECK (UnitPrice >= 0),
                    Supplier TEXT NOT NULL DEFAULT '',
                    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                """;
            await command.ExecuteNonQueryAsync();
        }

        await using var seedCheck = connection.CreateCommand();
        seedCheck.CommandText = "SELECT EXISTS (SELECT 1 FROM Products);";
        if (Convert.ToInt32(await seedCheck.ExecuteScalarAsync()) == 1)
        {
            return;
        }

        var samples = new[]
        {
            new ProductFormModel { Sku = "EL-1001", Name = "Wireless Mouse", Category = "Electronics", Quantity = 42, ReorderLevel = 10, UnitPrice = 24.99m, Supplier = "Northstar Supply" },
            new ProductFormModel { Sku = "EL-1002", Name = "USB-C Hub", Category = "Electronics", Quantity = 8, ReorderLevel = 12, UnitPrice = 39.50m, Supplier = "Northstar Supply" },
            new ProductFormModel { Sku = "OF-2001", Name = "Desk Organizer", Category = "Office", Quantity = 26, ReorderLevel = 8, UnitPrice = 18.75m, Supplier = "Paper & Co." },
            new ProductFormModel { Sku = "OF-2002", Name = "Notebook Set", Category = "Office", Quantity = 5, ReorderLevel = 10, UnitPrice = 12.00m, Supplier = "Paper & Co." },
            new ProductFormModel { Sku = "AC-3001", Name = "Laptop Stand", Category = "Accessories", Quantity = 17, ReorderLevel = 6, UnitPrice = 54.00m, Supplier = "Forma Goods" }
        };

        await using var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
            INSERT INTO Products (Sku, Name, Category, Quantity, ReorderLevel, UnitPrice, Supplier)
            VALUES ($sku, $name, $category, $quantity, $reorderLevel, $unitPrice, $supplier);
            """;

        foreach (var sample in samples)
        {
            seedCommand.Parameters.Clear();
            AddProductParameters(seedCommand, sample);
            await seedCommand.ExecuteNonQueryAsync();
        }
    }

    public async Task<InventoryDashboardViewModel> GetDashboardAsync(string? search)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        int totalProducts;
        int totalUnits;
        decimal inventoryValue;
        int lowStockCount;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT COUNT(*), COALESCE(SUM(Quantity), 0),
                       COALESCE(SUM(Quantity * UnitPrice), 0),
                       COALESCE(SUM(CASE WHEN Quantity <= ReorderLevel THEN 1 ELSE 0 END), 0)
                FROM Products;
                """;
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            totalProducts = reader.GetInt32(0);
            totalUnits = reader.GetInt32(1);
            inventoryValue = reader.GetDecimal(2);
            lowStockCount = reader.GetInt32(3);
        }

        var products = new List<Product>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, Sku, Name, Category, Quantity, ReorderLevel, UnitPrice, Supplier
                FROM Products
                WHERE $search = '' OR Sku LIKE $pattern OR Name LIKE $pattern OR Category LIKE $pattern OR Supplier LIKE $pattern
                ORDER BY CASE WHEN Quantity <= ReorderLevel THEN 0 ELSE 1 END, Name COLLATE NOCASE;
                """;
            var normalizedSearch = search?.Trim() ?? string.Empty;
            command.Parameters.AddWithValue("$search", normalizedSearch);
            command.Parameters.AddWithValue("$pattern", $"%{normalizedSearch}%");

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(ReadProduct(reader));
            }
        }

        return new InventoryDashboardViewModel
        {
            Products = products,
            TotalProducts = totalProducts,
            TotalUnits = totalUnits,
            InventoryValue = inventoryValue,
            LowStockCount = lowStockCount,
            Search = search
        };
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Sku, Name, Category, Quantity, ReorderLevel, UnitPrice, Supplier
            FROM Products WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadProduct(reader) : null;
    }

    public async Task<int> CreateAsync(ProductFormModel product)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Products (Sku, Name, Category, Quantity, ReorderLevel, UnitPrice, Supplier)
            VALUES ($sku, $name, $category, $quantity, $reorderLevel, $unitPrice, $supplier);
            SELECT last_insert_rowid();
            """;
        AddProductParameters(command, product);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(int id, ProductFormModel product)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Products
            SET Sku = $sku, Name = $name, Category = $category, Quantity = $quantity,
                ReorderLevel = $reorderLevel, UnitPrice = $unitPrice, Supplier = $supplier,
                UpdatedAt = CURRENT_TIMESTAMP
            WHERE Id = $id;
            """;
        AddProductParameters(command, product);
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> SkuExistsAsync(string? sku, int? exceptId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1 FROM Products
                WHERE Sku = $sku AND ($exceptId IS NULL OR Id <> $exceptId)
            );
            """;
        command.Parameters.AddWithValue("$sku", sku.Trim());
        command.Parameters.AddWithValue("$exceptId", (object?)exceptId ?? DBNull.Value);
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    private static void AddProductParameters(SqliteCommand command, ProductFormModel product)
    {
        command.Parameters.AddWithValue("$sku", product.Sku.Trim());
        command.Parameters.AddWithValue("$name", product.Name.Trim());
        command.Parameters.AddWithValue("$category", product.Category.Trim());
        command.Parameters.AddWithValue("$quantity", product.Quantity);
        command.Parameters.AddWithValue("$reorderLevel", product.ReorderLevel);
        command.Parameters.AddWithValue("$unitPrice", product.UnitPrice);
        command.Parameters.AddWithValue("$supplier", product.Supplier.Trim());
    }

    private static Product ReadProduct(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Sku = reader.GetString(1),
        Name = reader.GetString(2),
        Category = reader.GetString(3),
        Quantity = reader.GetInt32(4),
        ReorderLevel = reader.GetInt32(5),
        UnitPrice = reader.GetDecimal(6),
        Supplier = reader.GetString(7)
    };
}
