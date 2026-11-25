using System.Globalization;
using Microsoft.Data.Sqlite;
using StudentManagementApi.Models;

namespace StudentManagementApi.Data;

public sealed class StudentRepository
{
    private readonly string connectionString;

    public StudentRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("StudentDatabase")
            ?? throw new InvalidOperationException("The StudentDatabase connection string is missing.");
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
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentNumber TEXT NOT NULL COLLATE NOCASE UNIQUE,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                Department TEXT NOT NULL,
                DateOfBirth TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(string? search)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        var normalizedSearch = search?.Trim() ?? string.Empty;
        command.CommandText = """
            SELECT Id, StudentNumber, FirstName, LastName, Email, Department, DateOfBirth
            FROM Students
            WHERE $search = ''
               OR StudentNumber LIKE $pattern
               OR FirstName LIKE $pattern
               OR LastName LIKE $pattern
               OR Email LIKE $pattern
               OR Department LIKE $pattern
            ORDER BY LastName COLLATE NOCASE, FirstName COLLATE NOCASE, Id;
            """;
        command.Parameters.AddWithValue("$search", normalizedSearch);
        command.Parameters.AddWithValue("$pattern", $"%{normalizedSearch}%");

        var students = new List<Student>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(ReadStudent(reader));
        }

        return students;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, StudentNumber, FirstName, LastName, Email, Department, DateOfBirth
            FROM Students
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadStudent(reader) : null;
    }

    public async Task<Student?> CreateAsync(StudentRequest request)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Students (StudentNumber, FirstName, LastName, Email, Department, DateOfBirth)
            VALUES ($studentNumber, $firstName, $lastName, $email, $department, $dateOfBirth)
            RETURNING Id, StudentNumber, FirstName, LastName, Email, Department, DateOfBirth;
            """;
        AddStudentParameters(command, request);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadStudent(reader) : null;
        }
        catch (SqliteException exception) when (IsUniqueConstraintViolation(exception))
        {
            return null;
        }
    }

    public async Task<Student?> UpdateAsync(int id, StudentRequest request)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Students
            SET StudentNumber = $studentNumber,
                FirstName = $firstName,
                LastName = $lastName,
                Email = $email,
                Department = $department,
                DateOfBirth = $dateOfBirth
            WHERE Id = $id
            RETURNING Id, StudentNumber, FirstName, LastName, Email, Department, DateOfBirth;
            """;
        command.Parameters.AddWithValue("$id", id);
        AddStudentParameters(command, request);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadStudent(reader) : null;
        }
        catch (SqliteException exception) when (IsUniqueConstraintViolation(exception))
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static void AddStudentParameters(SqliteCommand command, StudentRequest request)
    {
        command.Parameters.AddWithValue("$studentNumber", request.StudentNumber.Trim());
        command.Parameters.AddWithValue("$firstName", request.FirstName.Trim());
        command.Parameters.AddWithValue("$lastName", request.LastName.Trim());
        command.Parameters.AddWithValue("$email", request.Email.Trim());
        command.Parameters.AddWithValue("$department", request.Department.Trim());
        command.Parameters.AddWithValue(
            "$dateOfBirth",
            request.DateOfBirth!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    private static Student ReadStudent(SqliteDataReader reader) => new(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.GetString(4),
        reader.GetString(5),
        DateOnly.ParseExact(reader.GetString(6), "yyyy-MM-dd", CultureInfo.InvariantCulture));

    private static bool IsUniqueConstraintViolation(SqliteException exception) =>
        exception.SqliteErrorCode == 19
        && exception.SqliteExtendedErrorCode is 1555 or 2067;
}
