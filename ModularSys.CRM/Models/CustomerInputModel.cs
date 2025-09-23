using System.ComponentModel.DataAnnotations;

namespace ModularSys.CRM.Models
{
    public class CustomerInputModel
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

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [StringLength(50, ErrorMessage = "Industry cannot exceed 50 characters")]
        public string? Industry { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Company size must be greater than 0")]
        public int? CompanySize { get; set; }

        [Url(ErrorMessage = "Invalid website URL")]
        [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters")]
        public string? Website { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        public string Status { get; set; } = "Active";
        public string CustomerType { get; set; } = "Prospect";
    }
}
