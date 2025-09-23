using System.ComponentModel.DataAnnotations;

namespace ModularSys.CRM.Models
{
    public class ContactInputModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; }

        [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
        public string? Department { get; set; }

        public bool IsPrimary { get; set; } = false;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public string Status { get; set; } = "Active";
    }
}
