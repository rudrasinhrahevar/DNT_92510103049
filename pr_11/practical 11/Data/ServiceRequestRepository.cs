using Microsoft.Data.Sqlite;
using SmartCampusPortal.Models;

namespace SmartCampusPortal.Data;

public sealed class ServiceRequestRepository
{
    private readonly string connectionString;

    public ServiceRequestRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("CampusDatabase")
            ?? throw new InvalidOperationException("The CampusDatabase connection string is missing.");
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
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ServiceRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                Location TEXT NOT NULL,
                Priority TEXT NOT NULL CHECK (Priority IN ('Low', 'Medium', 'High', 'Urgent')),
                RequesterName TEXT NOT NULL,
                RequesterEmail TEXT NOT NULL COLLATE NOCASE,
                Status TEXT NOT NULL DEFAULT 'Submitted'
                    CHECK (Status IN ('Submitted', 'In Progress', 'Resolved', 'Cancelled')),
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS IX_ServiceRequests_Status_CreatedAt
                ON ServiceRequests (Status, CreatedAt DESC);
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<CampusDashboardViewModel> GetDashboardAsync()
    {
        await using var connection = await OpenConnectionAsync();
        var totals = new int[4];
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT COUNT(*),
                       COALESCE(SUM(CASE WHEN Status = 'Submitted' THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status = 'In Progress' THEN 1 ELSE 0 END), 0),
                       COALESCE(SUM(CASE WHEN Status = 'Resolved' THEN 1 ELSE 0 END), 0)
                FROM ServiceRequests;
                """;
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            for (var index = 0; index < totals.Length; index++)
            {
                totals[index] = reader.GetInt32(index);
            }
        }

        var recentRequests = await GetRequestsAsync(connection, null, null, 6);
        return new CampusDashboardViewModel
        {
            TotalRequests = totals[0],
            OpenRequests = totals[1],
            InProgressRequests = totals[2],
            ResolvedRequests = totals[3],
            RecentRequests = recentRequests
        };
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetRequestsAsync(
        string? search = null,
        string? status = null,
        int? limit = null)
    {
        await using var connection = await OpenConnectionAsync();
        return await GetRequestsAsync(connection, search, status, limit);
    }

    public async Task<ServiceRequest?> GetByIdAsync(int id)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Category, Location, Priority,
                   RequesterName, RequesterEmail, Status, CreatedAt, UpdatedAt
            FROM ServiceRequests
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadRequest(reader) : null;
    }

    public async Task<int> CreateAsync(ServiceRequestForm request)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ServiceRequests
                (Title, Description, Category, Location, Priority, RequesterName, RequesterEmail)
            VALUES
                ($title, $description, $category, $location, $priority, $requesterName, $requesterEmail);
            SELECT last_insert_rowid();
            """;
        AddFormParameters(command, request);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE ServiceRequests
            SET Status = $status, UpdatedAt = CURRENT_TIMESTAMP
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$status", ServiceRequestStatuses.Normalize(status));
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM ServiceRequests WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() == 1;
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<IReadOnlyList<ServiceRequest>> GetRequestsAsync(
        SqliteConnection connection,
        string? search,
        string? status,
        int? limit)
    {
        var requests = new List<ServiceRequest>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Category, Location, Priority,
                   RequesterName, RequesterEmail, Status, CreatedAt, UpdatedAt
            FROM ServiceRequests
            WHERE ($search = '' OR Title LIKE $pattern OR Category LIKE $pattern
                   OR Location LIKE $pattern OR RequesterName LIKE $pattern
                   OR RequesterEmail LIKE $pattern)
              AND ($status = '' OR Status = $status COLLATE NOCASE)
            ORDER BY
                CASE Priority WHEN 'Urgent' THEN 0 WHEN 'High' THEN 1
                              WHEN 'Medium' THEN 2 ELSE 3 END,
                CreatedAt DESC
            LIMIT COALESCE($limit, -1);
            """;
        var normalizedSearch = search?.Trim() ?? string.Empty;
        command.Parameters.AddWithValue("$search", normalizedSearch);
        command.Parameters.AddWithValue("$pattern", $"%{normalizedSearch}%");
        command.Parameters.AddWithValue("$status", status?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$limit", (object?)limit ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            requests.Add(ReadRequest(reader));
        }

        return requests;
    }

    private static void AddFormParameters(SqliteCommand command, ServiceRequestForm request)
    {
        command.Parameters.AddWithValue("$title", request.Title.Trim());
        command.Parameters.AddWithValue("$description", request.Description.Trim());
        command.Parameters.AddWithValue("$category", request.Category.Trim());
        command.Parameters.AddWithValue("$location", request.Location.Trim());
        command.Parameters.AddWithValue("$priority", request.Priority);
        command.Parameters.AddWithValue("$requesterName", request.RequesterName.Trim());
        command.Parameters.AddWithValue("$requesterEmail", request.RequesterEmail.Trim());
    }

    private static ServiceRequest ReadRequest(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        Description = reader.GetString(2),
        Category = reader.GetString(3),
        Location = reader.GetString(4),
        Priority = reader.GetString(5),
        RequesterName = reader.GetString(6),
        RequesterEmail = reader.GetString(7),
        Status = reader.GetString(8),
        CreatedAt = DateTime.Parse(reader.GetString(9), System.Globalization.CultureInfo.InvariantCulture),
        UpdatedAt = DateTime.Parse(reader.GetString(10), System.Globalization.CultureInfo.InvariantCulture)
    };
}
