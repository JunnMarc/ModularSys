using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Services
{
    public class CRMAnalyticsService : ICRMAnalyticsService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public CRMAnalyticsService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<SalesAnalytics>> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var analytics = new List<SalesAnalytics>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                var monthEnd = current.AddMonths(1).AddDays(-1);
                var period = current.ToString("MMM yyyy");

                using var context = _contextFactory.CreateDbContext();
                var wonOpportunities = await context.Opportunities
                    .Where(o => !o.IsDeleted && o.Stage == "Won" && 
                               o.ActualCloseDate >= current && o.ActualCloseDate <= monthEnd)
                    .ToListAsync();

                var lostOpportunities = await context.Opportunities
                    .Where(o => !o.IsDeleted && o.Stage == "Lost" && 
                               o.ActualCloseDate >= current && o.ActualCloseDate <= monthEnd)
                    .CountAsync();

                var revenue = wonOpportunities.Sum(o => o.Value);
                var dealsWon = wonOpportunities.Count;
                var averageDealSize = dealsWon > 0 ? revenue / dealsWon : 0;
                var winRate = (dealsWon + lostOpportunities) > 0 ? (double)dealsWon / (dealsWon + lostOpportunities) * 100 : 0;
                var avgSalesCycle = wonOpportunities.Any() ? 
                    (int)wonOpportunities.Average(o => (o.ActualCloseDate!.Value - (o.CreatedAt ?? DateTime.MinValue)).Days) : 0;

                analytics.Add(new SalesAnalytics
                {
                    Period = period,
                    Revenue = revenue,
                    DealsWon = dealsWon,
                    DealsLost = lostOpportunities,
                    AverageDealSize = averageDealSize,
                    WinRate = winRate,
                    SalesCycle = avgSalesCycle,
                    Forecast = revenue * 1.1m // Simple forecast
                });

                current = current.AddMonths(1);
            }

            return analytics;
        }

        public async Task<IEnumerable<CustomerAnalytics>> GetCustomerAnalyticsAsync()
        {
            var analytics = new List<CustomerAnalytics>();
            var now = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = now.AddMonths(-i).Date;
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var period = monthStart.ToString("MMM yyyy");

                using var context2 = _contextFactory.CreateDbContext();
                var newCustomers = await context2.Customers
                    .Where(c => !c.IsDeleted && c.CreatedAt >= monthStart && c.CreatedAt <= monthEnd)
                    .CountAsync();

                var activeCustomers = await context2.Customers
                    .Where(c => !c.IsDeleted && c.Status == "Active" && c.CreatedAt <= monthEnd)
                    .CountAsync();

                analytics.Add(new CustomerAnalytics
                {
                    Period = period,
                    NewCustomers = newCustomers,
                    ActiveCustomers = activeCustomers,
                    ChurnedCustomers = 0, // Calculate based on business logic
                    ChurnRate = 0,
                    CustomerLifetimeValue = 10000, // Calculate based on actual data
                    AverageCustomerValue = 5000,
                    CustomerSegment = "All Segments"
                });
            }

            return analytics;
        }

        public async Task<IEnumerable<LeadAnalytics>> GetLeadAnalyticsAsync()
        {
            var analytics = new List<LeadAnalytics>();
            var now = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = now.AddMonths(-i).Date;
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var period = monthStart.ToString("MMM yyyy");

                using var context3 = _contextFactory.CreateDbContext();
                var newLeads = await context3.Leads
                    .Where(l => !l.IsDeleted && l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                    .CountAsync();

                var qualifiedLeads = await context3.Leads
                    .Where(l => !l.IsDeleted && l.Status == "Qualified" && l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                    .CountAsync();

                var convertedLeads = await context3.Leads
                    .Where(l => !l.IsDeleted && l.Status == "Converted" && l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                    .CountAsync();

                var conversionRate = newLeads > 0 ? (double)convertedLeads / newLeads * 100 : 0;

                analytics.Add(new LeadAnalytics
                {
                    Period = period,
                    NewLeads = newLeads,
                    QualifiedLeads = qualifiedLeads,
                    ConvertedLeads = convertedLeads,
                    ConversionRate = conversionRate,
                    LeadSource = "All Sources",
                    LeadValue = 0, // Calculate based on estimated values
                    AverageLeadTime = 14 // Calculate based on actual data
                });
            }

            return analytics;
        }

        public async Task<IEnumerable<OpportunityAnalytics>> GetOpportunityAnalyticsAsync()
        {
            var analytics = new List<OpportunityAnalytics>();
            var now = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = now.AddMonths(-i).Date;
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var period = monthStart.ToString("MMM yyyy");

                using var context4 = _contextFactory.CreateDbContext();
                var newOpportunities = await context4.Opportunities
                    .Where(o => !o.IsDeleted && o.CreatedAt >= monthStart && o.CreatedAt <= monthEnd)
                    .CountAsync();

                var wonOpportunities = await context4.Opportunities
                    .Where(o => !o.IsDeleted && o.Stage == "Won" && o.ActualCloseDate >= monthStart && o.ActualCloseDate <= monthEnd)
                    .ToListAsync();

                var lostOpportunities = await context4.Opportunities
                    .Where(o => !o.IsDeleted && o.Stage == "Lost" && o.ActualCloseDate >= monthStart && o.ActualCloseDate <= monthEnd)
                    .ToListAsync();

                var opportunityValue = await context4.Opportunities
                    .Where(o => !o.IsDeleted && o.CreatedAt >= monthStart && o.CreatedAt <= monthEnd)
                    .SumAsync(o => o.Value);

                var wonValue = wonOpportunities.Sum(o => o.Value);
                var lostValue = lostOpportunities.Sum(o => o.Value);
                var winRate = (wonOpportunities.Count + lostOpportunities.Count) > 0 ? 
                    (double)wonOpportunities.Count / (wonOpportunities.Count + lostOpportunities.Count) * 100 : 0;

                var avgSalesCycle = wonOpportunities.Any() ? 
                    (int)wonOpportunities.Average(o => (o.ActualCloseDate!.Value - (o.CreatedAt ?? DateTime.MinValue)).Days) : 0;

                analytics.Add(new OpportunityAnalytics
                {
                    Period = period,
                    NewOpportunities = newOpportunities,
                    WonOpportunities = wonOpportunities.Count,
                    LostOpportunities = lostOpportunities.Count,
                    OpportunityValue = opportunityValue,
                    WonValue = wonValue,
                    LostValue = lostValue,
                    Stage = "All Stages",
                    WinRate = winRate,
                    AverageSalesCycle = avgSalesCycle
                });
            }

            return analytics;
        }

        public async Task<CustomerRetentionAnalytics> GetCustomerRetentionAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var totalCustomers = await context.Customers.Where(c => !c.IsDeleted).CountAsync();
            var activeCustomers = await context.Customers.Where(c => !c.IsDeleted && c.Status == "Active").CountAsync();
            
            var retentionRate = totalCustomers > 0 ? (double)activeCustomers / totalCustomers * 100 : 0;
            var churnRate = 100 - retentionRate;

            return new CustomerRetentionAnalytics
            {
                RetentionRate = retentionRate,
                ChurnRate = churnRate,
                CustomerLifetimeValue = 15000, // Calculate based on actual data
                AverageCustomerLifespan = 365, // Calculate based on actual data
                Cohorts = new List<RetentionCohort>() // Implement cohort analysis
            };
        }

        public async Task<SalesForecast> GetSalesForecastAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            var nextMonth = currentMonth.AddMonths(1);

            using var context2 = _contextFactory.CreateDbContext();
            var pipelineValue = await context2.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != "Won" && o.Stage != "Lost")
                .SumAsync(o => o.Value);

            var avgProbability = await context2.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != "Won" && o.Stage != "Lost")
                .AverageAsync(o => (double?)o.Probability) ?? 0;

            var forecastRevenue = pipelineValue * (decimal)(avgProbability / 100);

            var actualRevenue = await context2.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == "Won" && o.ActualCloseDate >= currentMonth)
                .SumAsync(o => o.Value);

            return new SalesForecast
            {
                Period = nextMonth.ToString("MMM yyyy"),
                ForecastRevenue = forecastRevenue,
                ActualRevenue = actualRevenue,
                Accuracy = 85.0, // Calculate based on historical data
                ForecastDeals = 25, // Calculate based on pipeline
                ActualDeals = 20, // Calculate based on actual data
                PipelineValue = pipelineValue,
                CloseProbability = avgProbability
            };
        }

        public async Task<IEnumerable<TopCustomer>> GetTopCustomersAsync(int count = 10)
        {
            using var context = _contextFactory.CreateDbContext();
            var customers = await context.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => new TopCustomer
                {
                    CustomerId = c.Id,
                    CompanyName = c.CompanyName,
                    ContactName = c.ContactName,
                    TotalRevenue = c.Opportunities.Where(o => o.Stage == "Won").Sum(o => o.Value),
                    TotalOrders = c.Opportunities.Where(o => o.Stage == "Won").Count(),
                    LastOrderDate = c.Opportunities.Where(o => o.Stage == "Won")
                        .OrderByDescending(o => o.ActualCloseDate)
                        .Select(o => o.ActualCloseDate!.Value)
                        .FirstOrDefault(),
                    Status = c.Status,
                    AverageOrderValue = c.Opportunities.Where(o => o.Stage == "Won").Any() ?
                        c.Opportunities.Where(o => o.Stage == "Won").Average(o => o.Value) : 0
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(count)
                .ToListAsync();

            return customers;
        }

        public async Task<IEnumerable<SalesPerformance>> GetSalesPerformanceAsync()
        {
            // This would typically integrate with a user/sales rep system
            // For now, return sample data based on AssignedTo field
            using var context = _contextFactory.CreateDbContext();
            var salesReps = await context.Opportunities
                .Where(o => !o.IsDeleted && !string.IsNullOrEmpty(o.AssignedTo))
                .GroupBy(o => o.AssignedTo)
                .Select(g => new SalesPerformance
                {
                    SalesRep = g.Key!,
                    Revenue = g.Where(o => o.Stage == "Won").Sum(o => o.Value),
                    DealsWon = g.Count(o => o.Stage == "Won"),
                    DealsLost = g.Count(o => o.Stage == "Lost"),
                    WinRate = g.Count(o => o.Stage == "Won" || o.Stage == "Lost") > 0 ?
                        (double)g.Count(o => o.Stage == "Won") / g.Count(o => o.Stage == "Won" || o.Stage == "Lost") * 100 : 0,
                    AverageDealSize = g.Where(o => o.Stage == "Won").Any() ?
                        g.Where(o => o.Stage == "Won").Average(o => o.Value) : 0,
                    SalesCycle = 30, // Calculate based on actual data
                    Target = 100000, // Set based on business targets
                    TargetAchievement = 0 // Calculate based on target vs actual
                })
                .OrderByDescending(s => s.Revenue)
                .ToListAsync();

            // Calculate target achievement
            foreach (var rep in salesReps)
            {
                rep.TargetAchievement = rep.Target > 0 ? (double)(rep.Revenue / rep.Target) * 100 : 0;
            }

            return salesReps;
        }
    }
}
