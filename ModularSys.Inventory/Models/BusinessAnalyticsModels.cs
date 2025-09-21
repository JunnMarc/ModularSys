using System;
using System.Collections.Generic;

namespace ModularSys.Inventory.Models
{
    /// <summary>
    /// Comprehensive business analytics data for profit/loss analysis
    /// </summary>
    public class BusinessAnalyticsData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal LossAmount { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyBusinessMetrics> DailyMetrics { get; set; } = new();
        public List<CancellationAnalysis> CancellationReasons { get; set; } = new();
        public List<ProductProfitability> TopProfitableProducts { get; set; } = new();
        public List<ProductProfitability> LeastProfitableProducts { get; set; } = new();
    }

    /// <summary>
    /// Daily business metrics for trend analysis
    /// </summary>
    public class DailyBusinessMetrics
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Profit { get; set; }
        public decimal Loss { get; set; }
        public int OrdersCompleted { get; set; }
        public int OrdersCancelled { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public bool IsProfitable => Profit > 0;
        public decimal ProfitMargin => Revenue > 0 ? (Profit / Revenue) * 100 : 0;
    }

    /// <summary>
    /// Analysis of order cancellations and their reasons
    /// </summary>
    public class CancellationAnalysis
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal LostRevenue { get; set; }
        public DateTime? FirstOccurrence { get; set; }
        public DateTime? LastOccurrence { get; set; }
        public string Impact { get; set; } = string.Empty; // High, Medium, Low
    }

    /// <summary>
    /// Product profitability analysis
    /// </summary>
    public class ProductProfitability
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public int UnitsSold { get; set; }
        public int UnitsReturned { get; set; }
        public decimal AverageSellingPrice { get; set; }
        public decimal AverageCost { get; set; }
        public int CurrentStock { get; set; }
        public string ProfitabilityRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public string Rating { get; set; } = string.Empty; // Alias for ProfitabilityRating
    }

    /// <summary>
    /// Period comparison analytics
    /// </summary>
    public class PeriodComparison
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Profit { get; set; }
        public int Orders { get; set; }
        public int Cancellations { get; set; }
        public decimal GrowthRate { get; set; }
        public string Trend { get; set; } = string.Empty; // Improving, Declining, Stable
    }

    /// <summary>
    /// Customer behavior analytics
    /// </summary>
    public class CustomerAnalytics
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CancellationRate { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string CustomerSegment { get; set; } = string.Empty; // VIP, Regular, New, At-Risk
    }

    /// <summary>
    /// Inventory turnover and efficiency metrics
    /// </summary>
    public class InventoryEfficiency
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal AverageInventoryValue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal InventoryTurnoverRatio { get; set; }
        public int DaysInInventory { get; set; }
        public decimal StockoutRate { get; set; }
        public decimal CarryingCost { get; set; }
        public string EfficiencyRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
    }

    /// <summary>
    /// Financial health indicators
    /// </summary>
    public class FinancialHealthMetrics
    {
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public decimal InventoryTurnover { get; set; }
        public decimal CashFlowFromOperations { get; set; }
        public string OverallHealthScore { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
        public List<string> Recommendations { get; set; } = new();
    }
}
