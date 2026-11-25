using System.ComponentModel.DataAnnotations;

namespace SmartCampusPortal.Models;

public sealed class ServiceRequestForm
{
    [Required, StringLength(100, MinimumLength = 4)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Low|Medium|High|Urgent)$", ErrorMessage = "Select a valid priority.")]
    public string Priority { get; set; } = "Medium";

    [Required, StringLength(80)]
    [Display(Name = "Your name")]
    public string RequesterName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "Campus email")]
    public string RequesterEmail { get; set; } = string.Empty;
}

public sealed class UpdateRequestStatus
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
