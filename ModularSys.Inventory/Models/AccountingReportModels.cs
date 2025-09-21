using System.ComponentModel.DataAnnotations;

namespace ModularSys.Inventory.Models
{
    public class AccountingReportData
    {
        public string CompanyName { get; set; } = "ModuERP Systems";
        public string CompanyAddress { get; set; } = "123 Business District, Metro Manila, Philippines";
        public string CompanyPhone { get; set; } = "+63 2 8123-4567";
        public string CompanyEmail { get; set; } = "finance@moduerp.com";
        public string TaxId { get; set; } = "123-456-789-000";
        
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string ReportType { get; set; } = "Inventory Financial Report";
        public string PreparedBy { get; set; } = "System Administrator";
        
        // Financial Summary
        public FinancialSummary Summary { get; set; } = new();
        
        // Detailed Sections
        public List<RevenueBreakdown> RevenueBreakdown { get; set; } = new();
        public List<CostAnalysis> CostAnalysis { get; set; } = new();
        public List<ProductProfitability> ProductProfitability { get; set; } = new();
        public List<CategoryPerformance> CategoryPerformance { get; set; } = new();
        public List<InventoryValuation> InventoryValuation { get; set; } = new();
        public List<CashFlowItem> CashFlow { get; set; } = new();
        
        // Compliance & Notes
        public List<string> ComplianceNotes { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class FinancialSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitMargin { get; set; }
        
        public decimal TotalInventoryValue { get; set; }
        public decimal AverageInventoryTurnover { get; set; }
        public int TotalTransactions { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockItems { get; set; }
        
        public decimal TotalCancellations { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal CancellationImpact { get; set; }
    }

    public class RevenueBreakdown
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class CostAnalysis
    {
        public string CostCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryPerformance
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int UnitsSold { get; set; }
        public decimal InventoryValue { get; set; }
        public string Performance { get; set; } = string.Empty; // Excellent, Good, Average, Poor
    }

    public class InventoryValuation
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public decimal MarketValue { get; set; }
        public string ValuationMethod { get; set; } = "FIFO"; // FIFO, LIFO, Weighted Average
    }

    public class CashFlowItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
        public decimal NetFlow { get; set; }
        public decimal RunningBalance { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class ReportExportRequest
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string ReportTitle { get; set; } = "Inventory Financial Report";
        public string PreparedBy { get; set; } = "System Administrator";
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeDetailedBreakdown { get; set; } = true;
        public bool IncludeCashFlow { get; set; } = true;
        public bool IncludeInventoryValuation { get; set; } = true;
        public string ExportFormat { get; set; } = "PDF"; // PDF, Excel
        public string Currency { get; set; } = "PHP";
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
    }

    public class EnterpriseChartConfig
    {
        public string ChartType { get; set; } = string.Empty; // Line, Bar, Pie, Doughnut, Area
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public List<ChartData> Data { get; set; } = new();
        public string[] Colors { get; set; } = Array.Empty<string>();
        public bool ShowLegend { get; set; } = true;
        public bool ShowDataLabels { get; set; } = true;
        public string Height { get; set; } = "300px";
    }

    public class InventoryAccountingMetrics
    {
        public decimal BeginningInventoryValue { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal EndingInventoryValue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossSales { get; set; }
        public decimal SalesDiscounts { get; set; }
        public decimal NetSales { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal InventoryTurnoverRatio { get; set; }
        public decimal DaysSalesInInventory { get; set; }
        public decimal AverageInventory { get; set; }
        public int TotalTransactions { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockItems { get; set; }
        public int TotalCancellations { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal CancellationImpact { get; set; }
    }
}
