using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Interface
{
    public interface IBusinessAnalyticsService
    {
        /// <summary>
        /// Get comprehensive business analytics for the specified date range
        /// </summary>
        Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get customer analytics and behavior patterns
        /// </summary>
        Task<List<CustomerAnalytics>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate);
    }
}
