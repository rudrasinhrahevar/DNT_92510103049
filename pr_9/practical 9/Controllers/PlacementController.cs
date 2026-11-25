using System.Text;
using Microsoft.AspNetCore.Mvc;
using PlacementDashboard.Data;
using PlacementDashboard.Models;

namespace PlacementDashboard.Controllers;

public sealed class PlacementController(PlacementRepository repository) : Controller
{
    public async Task<IActionResult> Index(string? search, string? status, string? department)
    {
        return View(await repository.GetDashboardAsync(search, status, department));
    }

    public async Task<IActionResult> Report(string? search, string? status, string? department)
    {
        return View(await repository.GetDashboardAsync(search, status, department));
    }

    public async Task<IActionResult> ExportCsv(string? search, string? status, string? department)
    {
        var dashboard = await repository.GetDashboardAsync(search, status, department);
        var csv = new StringBuilder();
        csv.AppendLine("Student ID,Student,Department,Company,Role,CTC (LPA),Status,Updated");

        foreach (var placement in dashboard.Placements)
        {
            csv.AppendLine(string.Join(',',
                CsvField(placement.StudentId),
                CsvField(placement.StudentName),
                CsvField(placement.Department),
                CsvField(placement.Company),
                CsvField(placement.Role),
                CsvField(placement.CtcLpa?.ToString("0.00") ?? string.Empty),
                CsvField(placement.Status),
                CsvField(placement.UpdatedAt.ToString("yyyy-MM-dd"))));
        }

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8",
            "placement-report.csv");
    }

    private static string CsvField(string value)
    {
        var safeValue = value.Length > 0 && value[0] is '=' or '+' or '-' or '@'
            ? $"'{value}"
            : value;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}
