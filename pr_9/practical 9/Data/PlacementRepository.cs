using System.Globalization;
using Microsoft.Data.Sqlite;
using PlacementDashboard.Models;

namespace PlacementDashboard.Data;

public sealed class PlacementRepository
{
    private readonly string connectionString;

    public PlacementRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("PlacementDatabase")
            ?? throw new InvalidOperationException("The PlacementDatabase connection string is missing.");
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
                CREATE TABLE IF NOT EXISTS Placements (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId TEXT NOT NULL UNIQUE,
                    StudentName TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    Company TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    CtcLpa REAL NULL CHECK (CtcLpa IS NULL OR CtcLpa >= 0),
                    Status TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                """;
            await command.ExecuteNonQueryAsync();
        }

        await using var seedCheck = connection.CreateCommand();
        seedCheck.CommandText = "SELECT EXISTS (SELECT 1 FROM Placements);";
        if (Convert.ToInt32(await seedCheck.ExecuteScalarAsync()) == 1)
        {
            return;
        }

        var samples = new[]
        {
            new PlacementSeed("STU-2401", "Aarav Shah", "Computer Science", "Microsoft", "Software Engineer", 18.5m, "Placed", "2026-11-18"),
            new PlacementSeed("STU-2402", "Ananya Patel", "Information Technology", "Deloitte", "Technology Analyst", 9.2m, "Placed", "2026-11-17"),
            new PlacementSeed("STU-2403", "Dev Mehta", "Computer Science", "Atlassian", "Associate Developer", 14.0m, "Placed", "2026-11-16"),
            new PlacementSeed("STU-2404", "Isha Desai", "Electronics", "Siemens", "Graduate Engineer", 7.8m, "Placed", "2026-11-15"),
            new PlacementSeed("STU-2405", "Kabir Joshi", "Computer Science", "Google", "Associate Software Engineer", 24.0m, "Placed", "2026-11-14"),
            new PlacementSeed("STU-2406", "Mira Trivedi", "Information Technology", "TCS", "Systems Engineer", 6.5m, "Placed", "2026-11-13"),
            new PlacementSeed("STU-2407", "Rohan Shah", "Mechanical", "Tata Motors", "Design Engineer", 7.1m, "Placed", "2026-11-12"),
            new PlacementSeed("STU-2408", "Sara Khan", "Computer Science", "Adobe", "Product Engineer", 16.8m, "Offer Received", "2026-11-19"),
            new PlacementSeed("STU-2409", "Vihaan Patel", "Electronics", "Bosch", "Embedded Engineer", 8.4m, "Offer Received", "2026-11-18"),
            new PlacementSeed("STU-2410", "Zara Mehta", "Computer Science", "Amazon", "SDE Intern", null, "Interviewing", "2026-11-20"),
            new PlacementSeed("STU-2411", "Arjun Desai", "Information Technology", "Infosys", "Digital Specialist", null, "Interviewing", "2026-11-19"),
            new PlacementSeed("STU-2412", "Nisha Joshi", "Mechanical", "L&T", "Graduate Trainee", null, "Applied", "2026-11-20")
        };

        await using var insert = connection.CreateCommand();
        insert.CommandText = """
            INSERT INTO Placements (StudentId, StudentName, Department, Company, Role, CtcLpa, Status, UpdatedAt)
            VALUES ($studentId, $studentName, $department, $company, $role, $ctcLpa, $status, $updatedAt);
            """;

        foreach (var sample in samples)
        {
            insert.Parameters.Clear();
            insert.Parameters.AddWithValue("$studentId", sample.StudentId);
            insert.Parameters.AddWithValue("$studentName", sample.StudentName);
            insert.Parameters.AddWithValue("$department", sample.Department);
            insert.Parameters.AddWithValue("$company", sample.Company);
            insert.Parameters.AddWithValue("$role", sample.Role);
            insert.Parameters.AddWithValue("$ctcLpa", (object?)sample.CtcLpa ?? DBNull.Value);
            insert.Parameters.AddWithValue("$status", sample.Status);
            insert.Parameters.AddWithValue("$updatedAt", sample.UpdatedAt);
            await insert.ExecuteNonQueryAsync();
        }
    }

    public async Task<PlacementDashboardViewModel> GetDashboardAsync(
        string? search,
        string? status,
        string? department)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        int totalStudents;
        int placedStudents;
        int offerCount;
        int offerReceivedCount;
        int interviewingCount;
        int appliedCount;
        decimal averageCtcLpa;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT COUNT(*),
                       COALESCE(SUM(CASE WHEN Status = 'Placed' THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status IN ('Placed', 'Offer Received') THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status = 'Offer Received' THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status = 'Interviewing' THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status = 'Applied' THEN 1 ELSE 0 END), 0),
                       COALESCE(AVG(CASE WHEN Status = 'Placed' THEN CtcLpa END), 0)
                FROM Placements;
                """;
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            totalStudents = reader.GetInt32(0);
            placedStudents = reader.GetInt32(1);
            offerCount = reader.GetInt32(2);
            offerReceivedCount = reader.GetInt32(3);
            interviewingCount = reader.GetInt32(4);
            appliedCount = reader.GetInt32(5);
            averageCtcLpa = reader.GetDecimal(6);
        }

        var departments = new List<DepartmentSummary>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Department, COUNT(*),
                       SUM(CASE WHEN Status = 'Placed' THEN 1 ELSE 0 END)
                FROM Placements
                GROUP BY Department
                ORDER BY Department COLLATE NOCASE;
                """;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                departments.Add(new DepartmentSummary
                {
                    Name = reader.GetString(0),
                    StudentCount = reader.GetInt32(1),
                    PlacedCount = reader.GetInt32(2)
                });
            }
        }

        var placements = new List<Placement>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, StudentId, StudentName, Department, Company, Role, CtcLpa, Status, UpdatedAt
                FROM Placements
                WHERE ($search = ''
                       OR StudentId LIKE $pattern
                       OR StudentName LIKE $pattern
                       OR Department LIKE $pattern
                       OR Company LIKE $pattern
                       OR Role LIKE $pattern)
                  AND ($status = '' OR Status = $status)
                  AND ($department = '' OR Department = $department)
                ORDER BY CASE Status
                    WHEN 'Placed' THEN 0
                    WHEN 'Offer Received' THEN 1
                    WHEN 'Interviewing' THEN 2
                    ELSE 3
                END, StudentName COLLATE NOCASE;
                """;
            var normalizedSearch = search?.Trim() ?? string.Empty;
            var normalizedStatus = status?.Trim() ?? string.Empty;
            var normalizedDepartment = department?.Trim() ?? string.Empty;
            command.Parameters.AddWithValue("$search", normalizedSearch);
            command.Parameters.AddWithValue("$pattern", $"%{normalizedSearch}%");
            command.Parameters.AddWithValue("$status", normalizedStatus);
            command.Parameters.AddWithValue("$department", normalizedDepartment);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                placements.Add(ReadPlacement(reader));
            }
        }

        return new PlacementDashboardViewModel
        {
            Placements = placements,
            Departments = departments,
            TotalStudents = totalStudents,
            PlacedStudents = placedStudents,
            OfferCount = offerCount,
            OfferReceivedCount = offerReceivedCount,
            InterviewingCount = interviewingCount,
            AppliedCount = appliedCount,
            AverageCtcLpa = averageCtcLpa,
            FilteredCount = placements.Count,
            Search = search,
            Status = status,
            Department = department,
            DepartmentOptions = departments.Select(item => item.Name).ToArray(),
            StatusOptions = ["Placed", "Offer Received", "Interviewing", "Applied"]
        };
    }

    private static Placement ReadPlacement(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        StudentId = reader.GetString(1),
        StudentName = reader.GetString(2),
        Department = reader.GetString(3),
        Company = reader.GetString(4),
        Role = reader.GetString(5),
        CtcLpa = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
        Status = reader.GetString(7),
        UpdatedAt = DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture)
    };

    private sealed record PlacementSeed(
        string StudentId,
        string StudentName,
        string Department,
        string Company,
        string Role,
        decimal? CtcLpa,
        string Status,
        string UpdatedAt);
}
