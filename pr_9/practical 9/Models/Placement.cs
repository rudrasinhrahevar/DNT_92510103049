namespace PlacementDashboard.Models;

public sealed class Placement
{
    public int Id { get; init; }
    public string StudentId { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public decimal? CtcLpa { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
