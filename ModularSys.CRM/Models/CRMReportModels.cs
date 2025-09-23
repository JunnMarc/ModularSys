namespace ModularSys.CRM.Models
{
    public class CRMReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public CRMMetrics Metrics { get; set; } = new();
        public IEnumerable<CustomerSummary> Customers { get; set; } = new List<CustomerSummary>();
        public IEnumerable<LeadSummary> Leads { get; set; } = new List<LeadSummary>();
        public IEnumerable<OpportunitySummary> Opportunities { get; set; } = new List<OpportunitySummary>();
        public IEnumerable<SalesSummary> Sales { get; set; } = new List<SalesSummary>();
    }

    public class CRMMetrics
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public double LeadConversionRate { get; set; }
        public int TotalOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }
        public decimal TotalOpportunityValue { get; set; }
        public decimal WonOpportunityValue { get; set; }
        public decimal LostOpportunityValue { get; set; }
        public double OpportunityWinRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOpportunityValue { get; set; }
        public int AverageSalesCycle { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public double CustomerRetentionRate { get; set; }
    }

    public class CustomerSummary
    {
        public int CustomerId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOpportunities { get; set; }
        public DateTime LastActivityDate { get; set; }
    }

    public class LeadSummary
    {
        public int LeadId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LeadSource { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal? EstimatedValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class OpportunitySummary
    {
        public int OpportunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Stage { get; set; } = string.Empty;
        public int Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public string LeadSource { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    public class SalesSummary
    {
        public int OpportunityId { get; set; }
        public string OpportunityName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CloseDate { get; set; }
        public string SalesRep { get; set; } = string.Empty;
        public int SalesCycle { get; set; }
        public string LeadSource { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
    }

    public class EnterpriseChartConfig
    {
        public string ChartType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public IEnumerable<ChartData> Data { get; set; } = new List<ChartData>();
        public bool ShowLegend { get; set; } = true;
        public string Height { get; set; } = "300px";
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
    }
}
