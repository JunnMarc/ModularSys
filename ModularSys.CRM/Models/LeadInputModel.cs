using System.ComponentModel.DataAnnotations;

namespace ModularSys.CRM.Models
{
    public class LeadInputModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact name is required")]
        [StringLength(100, ErrorMessage = "Contact name cannot exceed 100 characters")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Lead source is required")]
        [StringLength(50, ErrorMessage = "Lead source cannot exceed 50 characters")]
        public string LeadSource { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Industry cannot exceed 50 characters")]
        public string? Industry { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Estimated value must be non-negative")]
        public decimal? EstimatedValue { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        public string Status { get; set; } = "New";
        public string Priority { get; set; } = "Medium";

        public DateTime? FollowUpDate { get; set; }
        public string? AssignedTo { get; set; }
    }
}
