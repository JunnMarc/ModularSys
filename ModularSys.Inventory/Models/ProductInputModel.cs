using System.ComponentModel.DataAnnotations;

namespace ModularSys.Inventory.Models
{
    public class ProductInputModel
    {
        public int ProductId { get; set; }

        public string? SKU { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers, and spaces are allowed.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Barcode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost price cannot be negative.")]
        public decimal? CostPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int QuantityOnHand { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
        public int ReorderLevel { get; set; }

        public int? MinStockLevel { get; set; }

        public int? MaxStockLevel { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string? BatchNumber { get; set; }

        public string? Supplier { get; set; }
    }
}
