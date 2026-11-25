namespace PlacementDashboard.Models;

public sealed class PlacementDashboardViewModel
{
    public IReadOnlyList<Placement> Placements { get; init; } = [];
    public IReadOnlyList<DepartmentSummary> Departments { get; init; } = [];
    public int TotalStudents { get; init; }
    public int PlacedStudents { get; init; }
    public int OfferCount { get; init; }
    public int OfferReceivedCount { get; init; }
    public int InterviewingCount { get; init; }
    public int AppliedCount { get; init; }
    public decimal AverageCtcLpa { get; init; }
    public int FilteredCount { get; init; }
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? Department { get; init; }
    public IReadOnlyList<string> DepartmentOptions { get; init; } = [];
    public IReadOnlyList<string> StatusOptions { get; init; } = [];
}

public sealed class DepartmentSummary
{
    public string Name { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public int PlacedCount { get; init; }
    public int PlacementRate => StudentCount == 0 ? 0 : (int)Math.Round(PlacedCount * 100d / StudentCount);
}
