using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface ICRMDashboardService
    {
        Task<CRMDashboardData> GetDashboardDataAsync();
        Task<IEnumerable<CustomerMetric>> GetCustomerMetricsAsync();
        Task<IEnumerable<LeadMetric>> GetLeadMetricsAsync();
        Task<IEnumerable<OpportunityMetric>> GetOpportunityMetricsAsync();
        Task<IEnumerable<SalesMetric>> GetSalesMetricsAsync();
        Task<IEnumerable<ActivityMetric>> GetActivityMetricsAsync();
        Task<decimal> GetMonthlyRevenueAsync();
        Task<decimal> GetQuarterlyRevenueAsync();
        Task<int> GetNewCustomersThisMonthAsync();
        Task<int> GetActiveLeadsAsync();
        Task<decimal> GetPipelineValueAsync();
        Task<double> GetConversionRateAsync();
    }
}
