namespace StudentManagementApi.Models;

public sealed record Student(
    int Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    DateOnly DateOfBirth);
