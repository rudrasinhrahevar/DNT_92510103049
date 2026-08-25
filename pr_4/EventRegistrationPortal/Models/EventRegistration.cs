using System.ComponentModel.DataAnnotations;

namespace EventRegistrationPortal.Models;

public class EventRegistration
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your email address.")]
    [Display(Name = "Confirm Email")]
    [Compare(nameof(Email), ErrorMessage = "Email and confirm email do not match.")]
    public string ConfirmEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age is required.")]
    [Range(18, 70, ErrorMessage = "Age must be between 18 and 70 years.")]
    [Display(Name = "Age")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Please select your gender.")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a dance style.")]
    [Display(Name = "Preferred Dance Style")]
    public string DanceStyle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your experience level.")]
    [Display(Name = "Experience Level")]
    public string ExperienceLevel { get; set; } = "Intermediate";

    [Required(ErrorMessage = "Please select a ticket tier.")]
    [Display(Name = "Ticket Tier")]
    public string TicketCategory { get; set; } = "Standard Pass ($49)";

    [Required(ErrorMessage = "Please select the event date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Event Date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Ticket quantity is required.")]
    [Range(1, 10, ErrorMessage = "You can register between 1 and 10 tickets.")]
    [Display(Name = "Number of Tickets")]
    public int NumberOfTickets { get; set; } = 1;

    [Display(Name = "I agree to the event rules and terms")]
    public bool TermsAccepted { get; set; }
}
