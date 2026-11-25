namespace SmartCampusPortal.Models;

public sealed class CampusDashboardViewModel
{
    public int TotalRequests { get; init; }
    public int OpenRequests { get; init; }
    public int InProgressRequests { get; init; }
    public int ResolvedRequests { get; init; }
    public IReadOnlyList<ServiceRequest> RecentRequests { get; init; } = [];
}

public sealed class ServiceRequestListViewModel
{
    public IReadOnlyList<ServiceRequest> Requests { get; init; } = [];
    public string? Search { get; init; }
    public string? Status { get; init; }
}
