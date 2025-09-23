using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Interface
{
    public interface IAnalyticalReportService
    {
        // Core Analytical Reports
        Task<InventoryValuationReport> GenerateInventoryValuationReportAsync(AnalyticalReportRequest request);
        Task<COGSReport> GenerateCOGSReportAsync(AnalyticalReportRequest request);
        Task<InventoryTurnoverReport> GenerateInventoryTurnoverReportAsync(AnalyticalReportRequest request);
        Task<StockAgingReport> GenerateStockAgingReportAsync(AnalyticalReportRequest request);
        Task<ProductProfitabilityReport> GenerateProductProfitabilityReportAsync(AnalyticalReportRequest request);
        
        // ERP Chart Generation
        Task<List<ERPChartConfig>> GenerateERPChartsAsync(AnalyticalReportRequest request);
        
        // Crystal Reports Integration (to be implemented)
        Task<byte[]> GenerateCrystalReportAsync(AnalyticalReportRequest request);
        Task<List<CrystalReportDefinition>> GetAvailableCrystalReportsAsync();
    }
}
