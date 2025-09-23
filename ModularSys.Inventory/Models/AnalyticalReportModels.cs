using System.ComponentModel.DataAnnotations;

namespace ModularSys.Inventory.Models
{
    // Main Report Types Enumeration
    public enum AnalyticalReportType
    {
        InventoryValuation,
        CostOfGoodsSold,
        InventoryTurnover,
        StockAging,
        ProfitabilityByProduct,
        PurchaseReport,
        SalesReport,
        AuditTrail,
        ComprehensiveFinancial
    }

    // Report Export Formats
    public enum ReportExportFormat
    {
        PDF,
        Excel,
        CrystalReport,
        CSV
    }

    // Enhanced Report Request Model
    public class AnalyticalReportRequest
    {
        public AnalyticalReportType ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportTitle { get; set; } = string.Empty;
        public string PreparedBy { get; set; } = "System Administrator";
        public ReportExportFormat ExportFormat { get; set; } = ReportExportFormat.PDF;
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeDetailedBreakdown { get; set; } = true;
        public bool IncludeComparativePeriod { get; set; } = false;
        public List<string>? SelectedCategories { get; set; }
        public List<string>? SelectedProducts { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
    }

    // 1. Inventory Valuation Report Model
    public class InventoryValuationReport
    {
        public DateTime ReportDate { get; set; }
        public string ValuationMethod { get; set; } = "Weighted Average"; // FIFO, LIFO, Weighted Average
        public decimal TotalInventoryValue { get; set; }
        public decimal BeginningInventoryValue { get; set; }
        public decimal EndingInventoryValue { get; set; }
        public decimal InventoryAdjustments { get; set; }
        public List<InventoryValuationItem> Items { get; set; } = new();
        public List<CategoryValuation> CategoryBreakdown { get; set; } = new();
        public InventoryValuationSummary Summary { get; set; } = new();
    }

    public class InventoryValuationItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastUpdated { get; set; }
        public string ValuationMethod { get; set; } = string.Empty;
        public decimal AverageAge { get; set; } // Days
    }

    public class CategoryValuation
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PercentageOfTotal { get; set; }
        public decimal AverageCost { get; set; }
    }

    public class InventoryValuationSummary
    {
        public decimal FastMovingValue { get; set; }
        public decimal SlowMovingValue { get; set; }
        public decimal ObsoleteValue { get; set; }
        public int TotalSKUs { get; set; }
        public decimal TurnoverRatio { get; set; }
        public decimal DaysOfSupply { get; set; }
    }

    // 2. Cost of Goods Sold (COGS) Report Model
    public class COGSReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal BeginningInventory { get; set; }
        public decimal Purchases { get; set; }
        public decimal EndingInventory { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossSales { get; set; }
        public decimal NetSales { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public List<COGSByCategory> CategoryBreakdown { get; set; } = new();
        public List<COGSByProduct> ProductBreakdown { get; set; } = new();
        public List<MonthlyCOGS> MonthlyTrend { get; set; } = new();
    }

    public class COGSByCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal COGS { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal PercentageOfTotalCOGS { get; set; }
    }

    public class COGSByProduct
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCOGS { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
    }

    public class MonthlyCOGS
    {
        public DateTime Month { get; set; }
        public decimal COGS { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
    }

    // 3. Inventory Turnover Report Model
    public class InventoryTurnoverReport
    {
        public DateTime ReportDate { get; set; }
        public decimal OverallTurnoverRatio { get; set; }
        public decimal AverageInventoryValue { get; set; }
        public decimal AnnualCOGS { get; set; }
        public decimal DaysSalesInInventory { get; set; }
        public List<ProductTurnover> ProductTurnover { get; set; } = new();
        public List<CategoryTurnover> CategoryTurnover { get; set; } = new();
        public TurnoverAnalysis Analysis { get; set; } = new();
    }

    public class ProductTurnover
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TurnoverRatio { get; set; }
        public decimal AverageInventory { get; set; }
        public decimal COGS { get; set; }
        public decimal DaysSalesInInventory { get; set; }
        public string TurnoverClassification { get; set; } = string.Empty; // Fast, Medium, Slow
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class CategoryTurnover
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TurnoverRatio { get; set; }
        public decimal AverageInventory { get; set; }
        public decimal COGS { get; set; }
        public decimal DaysSalesInInventory { get; set; }
        public int ProductCount { get; set; }
    }

    public class TurnoverAnalysis
    {
        public int FastMovingProducts { get; set; }
        public int MediumMovingProducts { get; set; }
        public int SlowMovingProducts { get; set; }
        public decimal FastMovingValue { get; set; }
        public decimal SlowMovingValue { get; set; }
        public decimal OptimizationOpportunity { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    // 4. Stock Aging Report Model
    public class StockAgingReport
    {
        public DateTime ReportDate { get; set; }
        public List<AgingBucket> AgingBuckets { get; set; } = new();
        public List<ProductAging> ProductAging { get; set; } = new();
        public AgingSummary Summary { get; set; } = new();
    }

    public class AgingBucket
    {
        public string BucketName { get; set; } = string.Empty; // 0-30 days, 31-60 days, etc.
        public int DaysFrom { get; set; }
        public int DaysTo { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class ProductAging
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastMovementDate { get; set; }
        public int DaysInStock { get; set; }
        public string AgingBucket { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public DateTime? ExpiryDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class AgingSummary
    {
        public decimal TotalInventoryValue { get; set; }
        public decimal FreshInventoryValue { get; set; }
        public decimal AgingInventoryValue { get; set; }
        public decimal ObsoleteInventoryValue { get; set; }
        public decimal PotentialWriteOff { get; set; }
        public int TotalSKUs { get; set; }
        public int AtRiskSKUs { get; set; }
    }

    // 5. Profitability by Product Report Model
    public class ProductProfitabilityReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProductProfitabilityDetail> Products { get; set; } = new();
        public List<CategoryProfitability> Categories { get; set; } = new();
        public ProfitabilitySummary Summary { get; set; } = new();
    }

    public class ProductProfitabilityDetail
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal COGS { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal AverageSellingPrice { get; set; }
        public decimal AverageCost { get; set; }
        public decimal ContributionMargin { get; set; }
        public string ProfitabilityRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public decimal ROI { get; set; }
        public int Ranking { get; set; }
    }

    public class CategoryProfitability
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal COGS { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public int ProductCount { get; set; }
        public int UnitsSold { get; set; }
        public decimal PercentageOfTotalRevenue { get; set; }
    }

    public class ProfitabilitySummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCOGS { get; set; }
        public decimal TotalGrossProfit { get; set; }
        public decimal OverallGrossProfitMargin { get; set; }
        public int HighProfitabilityProducts { get; set; }
        public int LowProfitabilityProducts { get; set; }
        public decimal TopProductsContribution { get; set; } // Top 20% products contribution
        public List<string> Recommendations { get; set; } = new();
    }

    // 6. Purchase Report Model
    public class PurchaseReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalShipping { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetPurchases { get; set; }
        public List<PurchaseOrderSummary> PurchaseOrders { get; set; } = new();
        public List<SupplierPurchaseSummary> SupplierSummary { get; set; } = new();
        public List<CategoryPurchaseSummary> CategorySummary { get; set; } = new();
        public List<MonthlyPurchaseTrend> MonthlyTrend { get; set; } = new();
    }

    public class PurchaseOrderSummary
    {
        public string PurchaseOrderId { get; set; } = string.Empty;
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int ItemCount { get; set; }
        public int DaysToReceive { get; set; }
    }

    public class SupplierPurchaseSummary
    {
        public string SupplierId { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PercentageOfTotalPurchases { get; set; }
        public decimal AverageDeliveryDays { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    public class CategoryPurchaseSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PercentageOfTotalPurchases { get; set; }
    }

    public class MonthlyPurchaseTrend
    {
        public DateTime Month { get; set; }
        public decimal TotalPurchases { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    // 7. Sales Report Model
    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal GrossSales { get; set; }
        public decimal SalesDiscounts { get; set; }
        public decimal SalesReturns { get; set; }
        public decimal NetSales { get; set; }
        public decimal SalesTax { get; set; }
        public List<SalesOrderSummary> SalesOrders { get; set; } = new();
        public List<CustomerSalesSummary> CustomerSummary { get; set; } = new();
        public List<CategorySalesSummary> CategorySummary { get; set; } = new();
        public List<MonthlySalesTrend> MonthlyTrend { get; set; } = new();
        public SalesPerformanceMetrics Performance { get; set; } = new();
    }

    public class SalesOrderSummary
    {
        public string SalesOrderId { get; set; } = string.Empty;
        public string SONumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int ItemCount { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class CustomerSalesSummary
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PercentageOfTotalSales { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string CustomerSegment { get; set; } = string.Empty; // VIP, Regular, New
    }

    public class CategorySalesSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public int UnitsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PercentageOfTotalSales { get; set; }
    }

    public class MonthlySalesTrend
    {
        public DateTime Month { get; set; }
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class SalesPerformanceMetrics
    {
        public decimal ConversionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal ReturnRate { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
    }

    // 8. Audit Trail Report Model
    public class AuditTrailReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<InventoryTransaction> Transactions { get; set; } = new();
        public List<TransactionSummary> Summary { get; set; } = new();
        public AuditStatistics Statistics { get; set; } = new();
    }

    public class InventoryTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Purchase, Sale, Adjustment, Transfer
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int BalanceAfter { get; set; }
    }

    public class TransactionSummary
    {
        public string TransactionType { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class AuditStatistics
    {
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
        public int UniqueProducts { get; set; }
        public int UniqueUsers { get; set; }
        public DateTime FirstTransaction { get; set; }
        public DateTime LastTransaction { get; set; }
        public decimal LargestTransaction { get; set; }
        public decimal SmallestTransaction { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }

    // Crystal Reports Integration Models
    public class CrystalReportDefinition
    {
        public string ReportName { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AnalyticalReportType ReportType { get; set; }
        public List<CrystalReportParameter> Parameters { get; set; } = new();
        public List<string> SupportedFormats { get; set; } = new();
    }

    public class CrystalReportParameter
    {
        public string ParameterName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Type DataType { get; set; } = typeof(string);
        public object? DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // Enhanced Chart Configuration for ERP-style reporting
    public class ERPChartConfig
    {
        public string ChartId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public ERPChartType ChartType { get; set; }
        public List<ERPChartSeries> Series { get; set; } = new();
        public ERPChartOptions Options { get; set; } = new();
        public string Height { get; set; } = "400px";
        public string Width { get; set; } = "100%";
        public bool ShowLegend { get; set; } = true;
        public bool ShowDataLabels { get; set; } = true;
        public string ColorScheme { get; set; } = "Professional"; // Professional, Vibrant, Monochrome
    }

    public enum ERPChartType
    {
        Line,
        Bar,
        Column,
        Pie,
        Doughnut,
        Area,
        Scatter,
        Combo,
        Waterfall,
        Gauge,
        Heatmap,
        Treemap
    }

    public class ERPChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<ERPChartDataPoint> Data { get; set; } = new();
        public string Color { get; set; } = string.Empty;
        public ERPChartType? Type { get; set; } // For combo charts
        public string YAxis { get; set; } = "primary"; // primary, secondary
    }

    public class ERPChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Category { get; set; }
        public string? Color { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ERPChartOptions
    {
        public ERPChartAxis XAxis { get; set; } = new();
        public ERPChartAxis YAxis { get; set; } = new();
        public ERPChartAxis? SecondaryYAxis { get; set; }
        public bool Responsive { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public bool EnableExport { get; set; } = true;
        public string Theme { get; set; } = "light";
        public ERPChartTooltip Tooltip { get; set; } = new();
    }

    public class ERPChartAxis
    {
        public string Title { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public bool ShowGrid { get; set; } = true;
        public string Position { get; set; } = "bottom"; // bottom, top, left, right
    }

    public class ERPChartTooltip
    {
        public bool Enabled { get; set; } = true;
        public string Format { get; set; } = "{series.name}: {point.y}";
        public bool Shared { get; set; } = false;
    }
}
