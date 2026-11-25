using System.ComponentModel.DataAnnotations;

namespace StudentManagementApi.Models;

public sealed class StudentRequest
{
    [Required, StringLength(20)]
    public string StudentNumber { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Department { get; init; } = string.Empty;

    [Required, Range(typeof(DateOnly), "1900-01-01", "2100-12-31")]
    public DateOnly? DateOfBirth { get; init; }
}
