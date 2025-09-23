using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface ICRMReportService
    {
        Task<byte[]> GenerateCustomerReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateLeadReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateOpportunityReportAsync(DateTime startDate, DateTime endDate);
        Task<CRMReportData> GetReportDataAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EnterpriseChartConfig>> GetCRMChartsAsync(DateTime startDate, DateTime endDate);
        Task ExportCustomerReportAsync(DateTime startDate, DateTime endDate);
        Task ExportSalesReportAsync(DateTime startDate, DateTime endDate);
    }
}
