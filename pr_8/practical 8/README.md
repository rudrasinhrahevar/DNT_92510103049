# Stockroom Inventory Management

A small ASP.NET Core MVC inventory app that demonstrates ADO.NET CRUD operations with SQLite. Products can be created, listed, searched, viewed, updated, and deleted. The dashboard tracks total products, units, stock value, and products at or below their reorder level.

## Run the application

From this directory:

```sh
dotnet run
```

The SQLite database is created automatically at `App_Data/inventory.db` and starts with sample products. The database file is local and excluded from source control.

## ADO.NET implementation

`Data/InventoryRepository.cs` uses `Microsoft.Data.Sqlite` connections, commands, parameters, and data readers directly. It creates the table and sample rows on startup; no ORM or migration tool is used. The connection string can be changed through `ConnectionStrings:InventoryDatabase` in `appsettings.json`.
