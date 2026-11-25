namespace SmartCampusPortal.Models;

public sealed class ServiceRequest
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string RequesterName { get; init; } = string.Empty;
    public string RequesterEmail { get; init; } = string.Empty;
    public string Status { get; init; } = ServiceRequestStatuses.Submitted;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public static class ServiceRequestStatuses
{
    public const string Submitted = "Submitted";
    public const string InProgress = "In Progress";
    public const string Resolved = "Resolved";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All =
    [
        Submitted,
        InProgress,
        Resolved,
        Cancelled
    ];

    public static bool IsValid(string? status) =>
        All.Contains(status, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string status) =>
        All.First(value => string.Equals(value, status, StringComparison.OrdinalIgnoreCase));
}
