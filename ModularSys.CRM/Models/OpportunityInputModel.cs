using System.ComponentModel.DataAnnotations;

namespace ModularSys.CRM.Models
{
    public class OpportunityInputModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Opportunity name is required")]
        [StringLength(200, ErrorMessage = "Opportunity name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Stage is required")]
        [StringLength(50, ErrorMessage = "Stage cannot exceed 50 characters")]
        public string Stage { get; set; } = "Prospecting";

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100")]
        public int Probability { get; set; } = 10;

        [Required(ErrorMessage = "Expected close date is required")]
        public DateTime ExpectedCloseDate { get; set; } = DateTime.Now.AddDays(30);

        [StringLength(50, ErrorMessage = "Lead source cannot exceed 50 characters")]
        public string? LeadSource { get; set; }

        [StringLength(100, ErrorMessage = "Competitor cannot exceed 100 characters")]
        public string? Competitor { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        public string? AssignedTo { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}
