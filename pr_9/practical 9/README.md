# Placement Dashboard

An ASP.NET Core MVC placement dashboard backed by SQLite. The dashboard demonstrates data-bound controls through searchable student records, department/status filters, KPI cards, department progress bars, and a hiring pipeline. The report page presents the same filtered records in a print-friendly format, and the CSV export downloads the selected result set.

## Run the application

From this directory:

```sh
dotnet run
```

The database is created at `App_Data/placements.db` and seeded with sample placement records on first launch. It is local and excluded from source control.

## Reports and Crystal Reports

The report page is designed for browser printing or saving as PDF, and CSV export is available for spreadsheet analysis. Native Crystal Reports integration is not included: SAP Crystal Reports runtime and designer target Windows-based .NET Framework environments and are not supported by this .NET 10 cross-platform application. To use Crystal Reports itself, host the report in a compatible Windows/.NET Framework application and connect it to the same placement data.

## Data access

`Data/PlacementRepository.cs` uses `Microsoft.Data.Sqlite` directly for schema initialization, sample data, dashboard aggregates, and parameterized filtering. Change `ConnectionStrings:PlacementDatabase` in `appsettings.json` to configure the database location.
