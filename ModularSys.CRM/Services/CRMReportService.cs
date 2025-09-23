using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ModularSys.CRM.Services
{
    public class CRMReportService : ICRMReportService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public CRMReportService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateCustomerReportAsync(DateTime startDate, DateTime endDate)
        {
            var reportData = await GetReportDataAsync(startDate, endDate);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Customer Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Report Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            x.Item().Text($"Total Customers: {reportData.Metrics.TotalCustomers}")
                                .FontSize(14).SemiBold();

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Company Name");
                                    header.Cell().Element(CellStyle).Text("Contact Name");
                                    header.Cell().Element(CellStyle).Text("Email");
                                    header.Cell().Element(CellStyle).Text("Industry");
                                    header.Cell().Element(CellStyle).Text("Status");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var customer in reportData.Customers.Take(50))
                                {
                                    table.Cell().Element(CellStyle).Text(customer.CompanyName);
                                    table.Cell().Element(CellStyle).Text(customer.ContactName);
                                    table.Cell().Element(CellStyle).Text(customer.Email);
                                    table.Cell().Element(CellStyle).Text(customer.Industry);
                                    table.Cell().Element(CellStyle).Text(customer.Status);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var reportData = await GetReportDataAsync(startDate, endDate);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Sales Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Report Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Total Revenue").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.TotalRevenue.ToString("C")).FontSize(16).FontColor(Colors.Green.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Won Opportunities").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.WonOpportunities.ToString()).FontSize(16).FontColor(Colors.Blue.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Win Rate").FontSize(12).SemiBold();
                                    col.Item().Text($"{reportData.Metrics.OpportunityWinRate:F1}%").FontSize(16).FontColor(Colors.Orange.Medium);
                                });
                            });

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Opportunity");
                                    header.Cell().Element(CellStyle).Text("Customer");
                                    header.Cell().Element(CellStyle).Text("Value");
                                    header.Cell().Element(CellStyle).Text("Close Date");
                                    header.Cell().Element(CellStyle).Text("Sales Rep");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var sale in reportData.Sales.Take(50))
                                {
                                    table.Cell().Element(CellStyle).Text(sale.OpportunityName);
                                    table.Cell().Element(CellStyle).Text(sale.CustomerName);
                                    table.Cell().Element(CellStyle).Text(sale.Value.ToString("C"));
                                    table.Cell().Element(CellStyle).Text(sale.CloseDate.ToString("MMM dd, yyyy"));
                                    table.Cell().Element(CellStyle).Text(sale.SalesRep);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateLeadReportAsync(DateTime startDate, DateTime endDate)
        {
            var reportData = await GetReportDataAsync(startDate, endDate);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Lead Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Report Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Total Leads").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.TotalLeads.ToString()).FontSize(16).FontColor(Colors.Blue.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Qualified Leads").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.QualifiedLeads.ToString()).FontSize(16).FontColor(Colors.Green.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Conversion Rate").FontSize(12).SemiBold();
                                    col.Item().Text($"{reportData.Metrics.LeadConversionRate:F1}%").FontSize(16).FontColor(Colors.Orange.Medium);
                                });
                            });

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Company");
                                    header.Cell().Element(CellStyle).Text("Contact");
                                    header.Cell().Element(CellStyle).Text("Email");
                                    header.Cell().Element(CellStyle).Text("Lead Source");
                                    header.Cell().Element(CellStyle).Text("Status");
                                    header.Cell().Element(CellStyle).Text("Priority");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var lead in reportData.Leads.Take(50))
                                {
                                    table.Cell().Element(CellStyle).Text(lead.CompanyName);
                                    table.Cell().Element(CellStyle).Text(lead.ContactName);
                                    table.Cell().Element(CellStyle).Text(lead.Email);
                                    table.Cell().Element(CellStyle).Text(lead.LeadSource);
                                    table.Cell().Element(CellStyle).Text(lead.Status);
                                    table.Cell().Element(CellStyle).Text(lead.Priority);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateOpportunityReportAsync(DateTime startDate, DateTime endDate)
        {
            var reportData = await GetReportDataAsync(startDate, endDate);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Opportunity Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Report Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(12);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Total Value").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.TotalOpportunityValue.ToString("C")).FontSize(16).FontColor(Colors.Green.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Won Value").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.WonOpportunityValue.ToString("C")).FontSize(16).FontColor(Colors.Blue.Medium);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Average Value").FontSize(12).SemiBold();
                                    col.Item().Text(reportData.Metrics.AverageOpportunityValue.ToString("C")).FontSize(16).FontColor(Colors.Orange.Medium);
                                });
                            });

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Opportunity");
                                    header.Cell().Element(CellStyle).Text("Customer");
                                    header.Cell().Element(CellStyle).Text("Value");
                                    header.Cell().Element(CellStyle).Text("Stage");
                                    header.Cell().Element(CellStyle).Text("Probability");
                                    header.Cell().Element(CellStyle).Text("Priority");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var opportunity in reportData.Opportunities.Take(50))
                                {
                                    table.Cell().Element(CellStyle).Text(opportunity.Name);
                                    table.Cell().Element(CellStyle).Text(opportunity.CustomerName);
                                    table.Cell().Element(CellStyle).Text(opportunity.Value.ToString("C"));
                                    table.Cell().Element(CellStyle).Text(opportunity.Stage);
                                    table.Cell().Element(CellStyle).Text($"{opportunity.Probability}%");
                                    table.Cell().Element(CellStyle).Text(opportunity.Priority);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<CRMReportData> GetReportDataAsync(DateTime startDate, DateTime endDate)
        {
            using var context = _contextFactory.CreateDbContext();
            var customers = await context.Customers
                .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .Select(c => new CustomerSummary
                {
                    CustomerId = c.Id,
                    CompanyName = c.CompanyName,
                    ContactName = c.ContactName,
                    Email = c.Email,
                    Phone = c.Phone ?? "",
                    Industry = c.Industry ?? "",
                    Status = c.Status,
                    CreatedDate = c.CreatedAt ?? DateTime.MinValue,
                    TotalRevenue = c.Opportunities.Where(o => o.Stage == "Won").Sum(o => o.Value),
                    TotalOpportunities = c.Opportunities.Count(),
                    LastActivityDate = c.UpdatedAt ?? c.CreatedAt ?? DateTime.MinValue
                })
                .ToListAsync();

            var leads = await context.Leads
                .Where(l => !l.IsDeleted && l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .Select(l => new LeadSummary
                {
                    LeadId = l.Id,
                    CompanyName = l.CompanyName,
                    ContactName = l.ContactName,
                    Email = l.Email,
                    Phone = l.Phone ?? "",
                    LeadSource = l.LeadSource,
                    Status = l.Status,
                    Priority = l.Priority,
                    EstimatedValue = l.EstimatedValue,
                    CreatedDate = l.CreatedAt ?? DateTime.MinValue,
                    FollowUpDate = l.FollowUpDate,
                    AssignedTo = l.AssignedTo ?? ""
                })
                .ToListAsync();

            var opportunities = await context.Opportunities
                .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .Include(o => o.Customer)
                .Select(o => new OpportunitySummary
                {
                    OpportunityId = o.Id,
                    Name = o.Name,
                    CustomerName = o.Customer.CompanyName,
                    Value = o.Value,
                    Stage = o.Stage,
                    Probability = o.Probability,
                    ExpectedCloseDate = o.ExpectedCloseDate,
                    CreatedDate = o.CreatedAt ?? DateTime.MinValue,
                    ActualCloseDate = o.ActualCloseDate,
                    LeadSource = o.LeadSource ?? "",
                    AssignedTo = o.AssignedTo ?? "",
                    Priority = o.Priority
                })
                .ToListAsync();

            var sales = await context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == "Won" && o.ActualCloseDate >= startDate && o.ActualCloseDate <= endDate)
                .Include(o => o.Customer)
                .Select(o => new SalesSummary
                {
                    OpportunityId = o.Id,
                    OpportunityName = o.Name,
                    CustomerName = o.Customer.CompanyName,
                    Value = o.Value,
                    CloseDate = o.ActualCloseDate!.Value,
                    SalesRep = o.AssignedTo ?? "",
                    SalesCycle = (int)(o.ActualCloseDate!.Value - (o.CreatedAt ?? DateTime.MinValue)).TotalDays,
                    LeadSource = o.LeadSource ?? "",
                    Industry = o.Customer.Industry ?? ""
                })
                .ToListAsync();

            var metrics = new CRMMetrics
            {
                TotalCustomers = customers.Count,
                NewCustomers = customers.Count,
                ActiveCustomers = customers.Count(c => c.Status == "Active"),
                TotalLeads = leads.Count,
                QualifiedLeads = leads.Count(l => l.Status == "Qualified"),
                ConvertedLeads = leads.Count(l => l.Status == "Converted"),
                LeadConversionRate = leads.Any() ? (double)leads.Count(l => l.Status == "Converted") / leads.Count * 100 : 0,
                TotalOpportunities = opportunities.Count,
                WonOpportunities = opportunities.Count(o => o.Stage == "Won"),
                LostOpportunities = opportunities.Count(o => o.Stage == "Lost"),
                TotalOpportunityValue = opportunities.Sum(o => o.Value),
                WonOpportunityValue = opportunities.Where(o => o.Stage == "Won").Sum(o => o.Value),
                LostOpportunityValue = opportunities.Where(o => o.Stage == "Lost").Sum(o => o.Value),
                OpportunityWinRate = opportunities.Count(o => o.Stage == "Won" || o.Stage == "Lost") > 0 ?
                    (double)opportunities.Count(o => o.Stage == "Won") / opportunities.Count(o => o.Stage == "Won" || o.Stage == "Lost") * 100 : 0,
                TotalRevenue = sales.Sum(s => s.Value),
                AverageOpportunityValue = opportunities.Any() ? opportunities.Average(o => o.Value) : 0,
                AverageSalesCycle = sales.Any() ? (int)sales.Average(s => s.SalesCycle) : 0,
                CustomerLifetimeValue = 15000, // Calculate based on actual data
                CustomerRetentionRate = 85.0 // Calculate based on actual data
            };

            return new CRMReportData
            {
                StartDate = startDate,
                EndDate = endDate,
                ReportType = "Comprehensive CRM Report",
                Metrics = metrics,
                Customers = customers,
                Leads = leads,
                Opportunities = opportunities,
                Sales = sales
            };
        }

        public async Task<IEnumerable<EnterpriseChartConfig>> GetCRMChartsAsync(DateTime startDate, DateTime endDate)
        {
            var charts = new List<EnterpriseChartConfig>();

            // Sales by Month Chart
            using var context = _contextFactory.CreateDbContext();
            var salesByMonth = await context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == "Won" && o.ActualCloseDate >= startDate && o.ActualCloseDate <= endDate)
                .GroupBy(o => new { o.ActualCloseDate!.Value.Year, o.ActualCloseDate.Value.Month })
                .Select(g => new ChartData
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:00}",
                    Value = g.Sum(o => o.Value),
                    Series = "Sales"
                })
                .ToListAsync();

            if (salesByMonth.Any())
            {
                charts.Add(new EnterpriseChartConfig
                {
                    ChartType = "Line",
                    Title = "Monthly Sales Revenue",
                    Subtitle = "Revenue trend over time",
                    Data = salesByMonth,
                    ShowLegend = false,
                    Height = "350px"
                });
            }

            // Lead Sources Chart
            using var context2 = _contextFactory.CreateDbContext();
            var leadSources = await context2.Leads
                .Where(l => !l.IsDeleted && l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .GroupBy(l => l.LeadSource)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count(),
                    Series = "Leads"
                })
                .ToListAsync();

            if (leadSources.Any())
            {
                charts.Add(new EnterpriseChartConfig
                {
                    ChartType = "Pie",
                    Title = "Lead Sources Distribution",
                    Subtitle = "Breakdown by lead source",
                    Data = leadSources,
                    ShowLegend = true,
                    Height = "350px"
                });
            }

            // Opportunity Stages Chart
            using var context3 = _contextFactory.CreateDbContext();
            var opportunityStages = await context3.Opportunities
                .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .GroupBy(o => o.Stage)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count(),
                    Series = "Opportunities"
                })
                .ToListAsync();

            if (opportunityStages.Any())
            {
                charts.Add(new EnterpriseChartConfig
                {
                    ChartType = "Doughnut",
                    Title = "Opportunity Pipeline",
                    Subtitle = "Opportunities by stage",
                    Data = opportunityStages,
                    ShowLegend = true,
                    Height = "350px"
                });
            }

            // Customer Growth Chart
            using var context4 = _contextFactory.CreateDbContext();
            var customerGrowth = await context4.Customers
                .Where(c => !c.IsDeleted && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .GroupBy(c => new { c.CreatedAt!.Value.Year, c.CreatedAt.Value.Month })
                .Select(g => new ChartData
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:00}",
                    Value = g.Count(),
                    Series = "Customers"
                })
                .ToListAsync();

            if (customerGrowth.Any())
            {
                charts.Add(new EnterpriseChartConfig
                {
                    ChartType = "Bar",
                    Title = "Customer Acquisition",
                    Subtitle = "New customers by month",
                    Data = customerGrowth,
                    ShowLegend = false,
                    Height = "350px"
                });
            }

            return charts;
        }

        public async Task ExportCustomerReportAsync(DateTime startDate, DateTime endDate)
        {
            var pdfBytes = await GenerateCustomerReportAsync(startDate, endDate);
            var fileName = $"CRM_Customer_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
        }

        public async Task ExportSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var pdfBytes = await GenerateSalesReportAsync(startDate, endDate);
            var fileName = $"CRM_Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
        }
    }
}
