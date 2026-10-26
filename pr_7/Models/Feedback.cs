using System;
using System.ComponentModel.DataAnnotations;

namespace pr_7.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        [Display(Name = "Student Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        [Display(Name = "Contact Number")]
        public string ContactNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a feedback category.")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        [Display(Name = "Rating (1 to 5)")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Comments are required.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Comments must be between 5 and 250 characters.")]
        [Display(Name = "Feedback Comments")]
        public string Comments { get; set; } = string.Empty;

        [Display(Name = "Preferred Contact Method")]
        public string ContactMethod { get; set; } = "Email";

        [Display(Name = "Would you recommend to others?")]
        public bool WouldRecommend { get; set; } = true;

        [Display(Name = "Submitted Date")]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
    }
}
