using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface ICRMAnalyticsService
    {
        Task<IEnumerable<SalesAnalytics>> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CustomerAnalytics>> GetCustomerAnalyticsAsync();
        Task<IEnumerable<LeadAnalytics>> GetLeadAnalyticsAsync();
        Task<IEnumerable<OpportunityAnalytics>> GetOpportunityAnalyticsAsync();
        Task<CustomerRetentionAnalytics> GetCustomerRetentionAsync();
        Task<SalesForecast> GetSalesForecastAsync();
        Task<IEnumerable<TopCustomer>> GetTopCustomersAsync(int count = 10);
        Task<IEnumerable<SalesPerformance>> GetSalesPerformanceAsync();
    }
}
