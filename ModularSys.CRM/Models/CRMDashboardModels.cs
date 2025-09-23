namespace ModularSys.CRM.Models
{
    public class CRMDashboardData
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int TotalLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ActiveLeads { get; set; }
        public int TotalOpportunities { get; set; }
        public decimal TotalOpportunityValue { get; set; }
        public decimal PipelineValue { get; set; }
        public int WonOpportunities { get; set; }
        public decimal WonOpportunityValue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal QuarterlyRevenue { get; set; }
        public double ConversionRate { get; set; }
        public double WinRate { get; set; }
        public decimal AverageOpportunityValue { get; set; }
        public int AverageSalesCycle { get; set; }
    }

    public class CustomerMetric
    {
        public string Period { get; set; } = string.Empty;
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int ChurnedCustomers { get; set; }
        public decimal CustomerValue { get; set; }
    }

    public class LeadMetric
    {
        public string Period { get; set; } = string.Empty;
        public int NewLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public double ConversionRate { get; set; }
        public string LeadSource { get; set; } = string.Empty;
    }

    public class OpportunityMetric
    {
        public string Period { get; set; } = string.Empty;
        public int NewOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }
        public decimal OpportunityValue { get; set; }
        public decimal WonValue { get; set; }
        public string Stage { get; set; } = string.Empty;
    }

    public class SalesMetric
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int DealsWon { get; set; }
        public int DealsLost { get; set; }
        public decimal AverageDealSize { get; set; }
        public int SalesCycle { get; set; }
        public string SalesRep { get; set; } = string.Empty;
    }

    public class ActivityMetric
    {
        public string Period { get; set; } = string.Empty;
        public int Calls { get; set; }
        public int Emails { get; set; }
        public int Meetings { get; set; }
        public int Tasks { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }
}
