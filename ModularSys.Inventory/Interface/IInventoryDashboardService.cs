using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Inventory.Interface
{
    public interface IInventoryDashboardService
    {
        Task<InventoryDashboardData> GetDashboardDataAsync();
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysAhead = 30);
        Task<IEnumerable<CategoryStockSummary>> GetCategoryStockSummaryAsync();
        Task<IEnumerable<InventoryMovementData>> GetInventoryMovementDataAsync(int days = 30);
        Task<IEnumerable<TopSellingProduct>> GetTopSellingProductsAsync(int days = 30, int count = 10);
        Task<RevenueAnalytics> GetRevenueAnalyticsAsync(int days = 30);
        Task<IEnumerable<StockAlert>> GetStockAlertsAsync();
    }

    public class InventoryDashboardData
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int ExpiringProductsCount { get; set; }
        public int PendingSalesOrders { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyCosts { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class CategoryStockSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
        public bool IsRevenueCritical { get; set; }
    }

    public class InventoryMovementData
    {
        public DateTime Date { get; set; }
        public int StockIn { get; set; }
        public int StockOut { get; set; }
        public decimal ValueIn { get; set; }
        public decimal ValueOut { get; set; }
        public string MovementType { get; set; } = string.Empty;
    }

    public class TopSellingProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal UnitPrice { get; set; }
        public int CurrentStock { get; set; }
    }

    public class RevenueAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public IEnumerable<DailyRevenue> DailyRevenues { get; set; } = new List<DailyRevenue>();
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Profit { get; set; }
    }

    public class StockAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string AlertType { get; set; } = string.Empty; // "LowStock", "OutOfStock", "Expiring", "Critical"
        public string Message { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int? ReorderLevel { get; set; }
        public int? MinStockLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Severity { get; set; } = "Info"; // "Info", "Warning", "Error", "Critical"
        public DateTime CreatedAt { get; set; }
    }
}
