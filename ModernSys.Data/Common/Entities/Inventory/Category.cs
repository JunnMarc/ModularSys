using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class Category : ISoftDeletable
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; } = "Category";
        public string? Color { get; set; } = "#1976d2";

        // If true, low stock in this category impacts revenue and triggers urgent restock
        public bool IsRevenueCritical { get; set; }

        // Default reorder threshold for items in this category
        public int DefaultMinThreshold { get; set; }
        
        // Category hierarchy support
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
