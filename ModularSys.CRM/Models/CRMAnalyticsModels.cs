namespace ModularSys.CRM.Models
{
    public class SalesAnalytics
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int DealsWon { get; set; }
        public int DealsLost { get; set; }
        public decimal AverageDealSize { get; set; }
        public double WinRate { get; set; }
        public int SalesCycle { get; set; }
        public decimal Forecast { get; set; }
    }

    public class CustomerAnalytics
    {
        public string Period { get; set; } = string.Empty;
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int ChurnedCustomers { get; set; }
        public double ChurnRate { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public string CustomerSegment { get; set; } = string.Empty;
    }

    public class LeadAnalytics
    {
        public string Period { get; set; } = string.Empty;
        public int NewLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public double ConversionRate { get; set; }
        public string LeadSource { get; set; } = string.Empty;
        public decimal LeadValue { get; set; }
        public int AverageLeadTime { get; set; }
    }

    public class OpportunityAnalytics
    {
        public string Period { get; set; } = string.Empty;
        public int NewOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }
        public decimal OpportunityValue { get; set; }
        public decimal WonValue { get; set; }
        public decimal LostValue { get; set; }
        public string Stage { get; set; } = string.Empty;
        public double WinRate { get; set; }
        public int AverageSalesCycle { get; set; }
    }

    public class CustomerRetentionAnalytics
    {
        public double RetentionRate { get; set; }
        public double ChurnRate { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public int AverageCustomerLifespan { get; set; }
        public IEnumerable<RetentionCohort> Cohorts { get; set; } = new List<RetentionCohort>();
    }

    public class RetentionCohort
    {
        public string CohortMonth { get; set; } = string.Empty;
        public int CustomersCount { get; set; }
        public double RetentionRate { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesForecast
    {
        public string Period { get; set; } = string.Empty;
        public decimal ForecastRevenue { get; set; }
        public decimal ActualRevenue { get; set; }
        public double Accuracy { get; set; }
        public int ForecastDeals { get; set; }
        public int ActualDeals { get; set; }
        public decimal PipelineValue { get; set; }
        public double CloseProbability { get; set; }
    }

    public class TopCustomer
    {
        public int CustomerId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AverageOrderValue { get; set; }
    }

    public class SalesPerformance
    {
        public string SalesRep { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int DealsWon { get; set; }
        public int DealsLost { get; set; }
        public double WinRate { get; set; }
        public decimal AverageDealSize { get; set; }
        public int SalesCycle { get; set; }
        public decimal Target { get; set; }
        public double TargetAchievement { get; set; }
    }
}
