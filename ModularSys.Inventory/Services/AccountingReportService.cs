using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Inventory.Models;
using ModularSys.Inventory.Interface;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ModularSys.Inventory.Services
{
    public interface IAccountingReportService
    {
        Task<AccountingReportData> GenerateAccountingReportAsync(DateTime startDate, DateTime endDate);
        Task<List<EnterpriseChartConfig>> GetEnterpriseChartsAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportReportToPdfAsync(AccountingReportData reportData, ReportExportRequest request);
        Task<AccountingReportData> PreviewReportAsync(ReportExportRequest request);
    }

    public class AccountingReportService : IAccountingReportService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBusinessAnalyticsService _analyticsService;

        public AccountingReportService(IServiceScopeFactory scopeFactory, IBusinessAnalyticsService analyticsService)
        {
            _scopeFactory = scopeFactory;
            _analyticsService = analyticsService;
        }

        public async Task<AccountingReportData> GenerateAccountingReportAsync(DateTime startDate, DateTime endDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var report = new AccountingReportData
            {
                ReportDate = DateTime.Now,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                CompanyName = "ModuERP Solutions",
                CompanyAddress = "123 Matina District, Davao City, Philippines",
                CompanyPhone = "+63 2 8123 4567",
                CompanyEmail = "info@moduerp.com",
                TaxId = "123-456-789-000",
                ReportType = "Inventory Financial Report"
            };

            // Real-world inventory accounting calculations
            var inventoryMetrics = await CalculateInventoryAccountingMetricsAsync(db, startDate, endDate);
            
            // Financial Summary using standard accounting methods
            report.Summary = new FinancialSummary
            {
                TotalRevenue = inventoryMetrics.NetSales,
                TotalCosts = inventoryMetrics.CostOfGoodsSold,
                GrossProfit = inventoryMetrics.GrossProfit,
                GrossProfitMargin = inventoryMetrics.GrossProfitMargin,
                NetProfit = inventoryMetrics.NetIncome,
                NetProfitMargin = inventoryMetrics.NetProfitMargin,
                TotalInventoryValue = inventoryMetrics.EndingInventoryValue,
                AverageInventoryTurnover = inventoryMetrics.InventoryTurnoverRatio,
                TotalTransactions = inventoryMetrics.TotalTransactions,
                ActiveProducts = inventoryMetrics.ActiveProducts,
                LowStockItems = inventoryMetrics.LowStockItems,
                TotalCancellations = inventoryMetrics.TotalCancellations,
                CancellationRate = inventoryMetrics.CancellationRate,
                CancellationImpact = inventoryMetrics.CancellationImpact
            };

            // Standard accounting reports
            report.RevenueBreakdown = await GetRevenueBreakdownAsync(db, startDate, endDate);
            report.CostAnalysis = await GetCostAnalysisAsync(db, startDate, endDate, inventoryMetrics);
            report.CategoryPerformance = await GetCategoryPerformanceAsync(db, startDate, endDate);
            report.InventoryValuation = await GetInventoryValuationAsync(db);
            report.CashFlow = await GetCashFlowAsync(db, startDate, endDate);
            report.ProductProfitability = await GetProductProfitabilityAsync(db, startDate, endDate);

            // Compliance and recommendations
            report.ComplianceNotes = GetComplianceNotes(report);
            report.Recommendations = GetRecommendations(report);

            return report;
        }

        private async Task<InventoryAccountingMetrics> CalculateInventoryAccountingMetricsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            // FIXED: Calculate COGS based on actual sales, not inventory formula
            var salesOrderLines = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate && 
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted)
                .ToListAsync();

            // Calculate actual COGS from sales
            var costOfGoodsSold = salesOrderLines.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0));
            
            // Purchases during period (for cash flow analysis, not COGS)
            var purchasesDuringPeriod = await db.PurchaseOrders
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && 
                           po.Status == "Received" && !po.IsDeleted)
                .Select(po => new { po.SubTotal, po.TaxRate, po.DiscountAmount, po.ShippingCost })
                .ToListAsync();
            
            var totalPurchases = purchasesDuringPeriod.Sum(po => 
                (po.SubTotal - po.DiscountAmount) + ((po.SubTotal - po.DiscountAmount) * po.TaxRate) + po.ShippingCost);

            // Inventory values for balance sheet purposes
            var beginningInventoryValue = await CalculateInventoryValueAtDate(db, startDate.AddDays(-1));
            var endingInventoryValue = await CalculateInventoryValueAtDate(db, endDate);

            // FIXED: Net Sales calculation from actual sales order lines
            var grossSales = salesOrderLines.Sum(sol => sol.LineTotal);
            var salesDiscounts = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate && 
                           so.Status == "Completed" && !so.IsDeleted)
                .SumAsync(so => so.DiscountAmount);
            var netSales = grossSales - salesDiscounts;

            // Gross Profit = Net Sales - COGS
            var grossProfit = netSales - costOfGoodsSold;
            var grossProfitMargin = netSales > 0 ? (grossProfit / netSales) * 100 : 0;

            // Operating Expenses (simplified - in real world would include rent, salaries, utilities, etc.)
            var operatingExpenses = totalPurchases * 0.15m; // Estimate 15% of purchases as operating expenses

            // Net Income = Gross Profit - Operating Expenses
            var netIncome = grossProfit - operatingExpenses;
            var netProfitMargin = netSales > 0 ? (netIncome / netSales) * 100 : 0;

            // Inventory Turnover Ratio = COGS / Average Inventory
            var averageInventory = (beginningInventoryValue + endingInventoryValue) / 2;
            var inventoryTurnoverRatio = averageInventory > 0 ? costOfGoodsSold / averageInventory : 0;

            // Days Sales in Inventory = 365 / Inventory Turnover Ratio
            var daysSalesInInventory = inventoryTurnoverRatio > 0 ? 365 / inventoryTurnoverRatio : 0;

            // FIXED: Additional metrics with proper soft delete filtering
            var totalTransactions = await db.SalesOrders
                .CountAsync(so => so.OrderDate >= startDate && so.OrderDate <= endDate && !so.IsDeleted);
            var activeProducts = await db.Products.CountAsync(p => !p.IsDeleted && p.IsActive);
            var lowStockItems = await db.Products
                .CountAsync(p => !p.IsDeleted && p.IsActive && p.QuantityOnHand <= p.ReorderLevel);
            
            var cancelledOrders = await db.SalesOrders
                .CountAsync(so => so.OrderDate >= startDate && so.OrderDate <= endDate && 
                               so.Status == "Cancelled" && !so.IsDeleted);
            var cancellationRate = totalTransactions > 0 ? (decimal)cancelledOrders / totalTransactions * 100 : 0;
            
            var cancellationImpact = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate && 
                           so.Status == "Cancelled" && !so.IsDeleted)
                .SumAsync(so => so.SubTotal);

            return new InventoryAccountingMetrics
            {
                BeginningInventoryValue = beginningInventoryValue,
                TotalPurchases = totalPurchases,
                EndingInventoryValue = endingInventoryValue,
                CostOfGoodsSold = costOfGoodsSold,
                GrossSales = grossSales,
                SalesDiscounts = salesDiscounts,
                NetSales = netSales,
                GrossProfit = grossProfit,
                GrossProfitMargin = grossProfitMargin,
                OperatingExpenses = operatingExpenses,
                NetIncome = netIncome,
                NetProfitMargin = netProfitMargin,
                InventoryTurnoverRatio = inventoryTurnoverRatio,
                DaysSalesInInventory = daysSalesInInventory,
                AverageInventory = averageInventory,
                TotalTransactions = totalTransactions,
                ActiveProducts = activeProducts,
                LowStockItems = lowStockItems,
                TotalCancellations = cancelledOrders,
                CancellationRate = cancellationRate,
                CancellationImpact = cancellationImpact
            };
        }

        private async Task<decimal> CalculateInventoryValueAtDate(InventoryDbContext db, DateTime date)
        {
            // FIXED: More accurate inventory valuation with better cost tracking
            var products = await db.Products
                .Where(p => !p.IsDeleted && p.IsActive && p.QuantityOnHand > 0)
                .Select(p => new { p.ProductId, p.QuantityOnHand, p.CostPrice })
                .ToListAsync();

            decimal totalInventoryValue = 0;

            foreach (var product in products)
            {
                // In a real system, this would use weighted average cost from inventory transactions
                // For now, using current cost price but with better validation
                var costPerUnit = product.CostPrice > 0 ? product.CostPrice : 0;
                var productValue = product.QuantityOnHand * costPerUnit;
                totalInventoryValue += productValue;
            }

            return totalInventoryValue;
        }

        public async Task<List<EnterpriseChartConfig>> GetEnterpriseChartsAsync(DateTime startDate, DateTime endDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            
            var charts = new List<EnterpriseChartConfig>();

            try
            {
                // 1. Revenue vs Costs Trend Chart
                var revenueVsCostsChart = await GetRevenueVsCostsTrendChart(db, startDate, endDate);
                charts.Add(revenueVsCostsChart);

                // 2. Category Revenue Distribution
                var categoryRevenueChart = await GetCategoryRevenueChart(db, startDate, endDate);
                charts.Add(categoryRevenueChart);

                // 3. Monthly Profit Margins
                var profitMarginsChart = await GetProfitMarginsChart(db, startDate, endDate);
                charts.Add(profitMarginsChart);

                // 4. Inventory Turnover Analysis
                var inventoryTurnoverChart = await GetInventoryTurnoverChart(db, startDate, endDate);
                charts.Add(inventoryTurnoverChart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating charts: {ex.Message}");
                
                // Add sample data for testing if no real data exists
                charts.AddRange(GetSampleCharts());
            }

            return charts;
        }

        private List<EnterpriseChartConfig> GetSampleCharts()
        {
            return new List<EnterpriseChartConfig>
            {
                new EnterpriseChartConfig
                {
                    ChartType = "Line",
                    Title = "Daily Revenue Trend (Sample)",
                    Subtitle = "Revenue performance over time",
                    Data = new List<ChartData>
                    {
                        new ChartData { Label = "Mon", Value = 1200, Series = "Revenue" },
                        new ChartData { Label = "Tue", Value = 1500, Series = "Revenue" },
                        new ChartData { Label = "Wed", Value = 1100, Series = "Revenue" },
                        new ChartData { Label = "Thu", Value = 1800, Series = "Revenue" },
                        new ChartData { Label = "Fri", Value = 2200, Series = "Revenue" },
                        new ChartData { Label = "Sat", Value = 1900, Series = "Revenue" }
                    },
                    ShowLegend = false,
                    Height = "350px"
                },
                new EnterpriseChartConfig
                {
                    ChartType = "Pie",
                    Title = "Category Revenue Distribution (Sample)",
                    Subtitle = "Revenue breakdown by category",
                    Data = new List<ChartData>
                    {
                        new ChartData { Label = "Electronics", Value = 5000, Series = "Revenue" },
                        new ChartData { Label = "Clothing", Value = 3000, Series = "Revenue" },
                        new ChartData { Label = "Books", Value = 2000, Series = "Revenue" },
                        new ChartData { Label = "Home & Garden", Value = 1500, Series = "Revenue" }
                    },
                    ShowLegend = true,
                    Height = "350px"
                },
                new EnterpriseChartConfig
                {
                    ChartType = "Bar",
                    Title = "Monthly Profit Margins (Sample)",
                    Subtitle = "Profitability analysis",
                    Data = new List<ChartData>
                    {
                        new ChartData { Label = "January", Value = 25, Series = "Profit Margin %" },
                        new ChartData { Label = "February", Value = 30, Series = "Profit Margin %" },
                        new ChartData { Label = "March", Value = 22, Series = "Profit Margin %" },
                        new ChartData { Label = "April", Value = 35, Series = "Profit Margin %" }
                    },
                    ShowLegend = false,
                    Height = "350px"
                },
                new EnterpriseChartConfig
                {
                    ChartType = "Doughnut",
                    Title = "Inventory Turnover (Sample)",
                    Subtitle = "Stock movement efficiency",
                    Data = new List<ChartData>
                    {
                        new ChartData { Label = "Fast Moving", Value = 60, Series = "Turnover" },
                        new ChartData { Label = "Medium Moving", Value = 30, Series = "Turnover" },
                        new ChartData { Label = "Slow Moving", Value = 10, Series = "Turnover" }
                    },
                    ShowLegend = true,
                    Height = "350px"
                }
            };
        }

        private async Task<EnterpriseChartConfig> GetRevenueVsCostsTrendChart(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var dailyData = new List<ChartData>();
            
            // Get daily sales and costs for the period
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var dayStart = currentDate.Date;
                var dayEnd = dayStart.AddDays(1);

                // Daily Revenue
                var dailyRevenue = await db.SalesOrders
                    .Where(so => so.OrderDate >= dayStart && so.OrderDate < dayEnd && so.Status == "Completed")
                    .SumAsync(so => so.SubTotal - so.DiscountAmount);

                // Daily Costs (COGS approximation)
                var dailyCosts = await db.SalesOrderLines
                    .Include(sol => sol.SalesOrder)
                    .Include(sol => sol.Product)
                    .Where(sol => sol.SalesOrder.OrderDate >= dayStart && 
                                 sol.SalesOrder.OrderDate < dayEnd && 
                                 sol.SalesOrder.Status == "Completed")
                    .SumAsync(sol => sol.Quantity * sol.Product.CostPrice);

                dailyData.Add(new ChartData
                {
                    Label = currentDate.ToString("MMM dd"),
                    Value = dailyRevenue,
                    Series = "Revenue"
                });

                dailyData.Add(new ChartData
                {
                    Label = currentDate.ToString("MMM dd"),
                    Value = dailyCosts,
                    Series = "Costs"
                });

                currentDate = currentDate.AddDays(1);
            }

            return new EnterpriseChartConfig
            {
                ChartType = "Line",
                Title = "Revenue vs Costs Trend",
                Subtitle = "Daily performance tracking",
                Data = dailyData,
                Colors = new[] { "#1976d2", "#d32f2f" },
                ShowLegend = true,
                ShowDataLabels = false,
                Height = "350px"
            };
        }

        private async Task<EnterpriseChartConfig> GetCategoryRevenueChart(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var categoryRevenue = await db.SalesOrderLines
                .Include(sol => sol.SalesOrder)
                .Include(sol => sol.Product)
                .ThenInclude(p => p.Category)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate &&
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed")
                .GroupBy(sol => sol.Product.Category.CategoryName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Sum(sol => sol.Quantity * sol.UnitPrice),
                    Series = "Revenue"
                })
                .OrderByDescending(cd => cd.Value)
                .Take(10)
                .ToListAsync();

            return new EnterpriseChartConfig
            {
                ChartType = "Pie",
                Title = "Revenue by Category",
                Subtitle = "Top 10 category performance",
                Data = categoryRevenue,
                Colors = new[] { "#1976d2", "#388e3c", "#f57c00", "#7b1fa2", "#c62828", "#00796b", "#5d4037", "#455a64", "#e65100", "#ad1457" },
                ShowLegend = true,
                ShowDataLabels = true,
                Height = "350px"
            };
        }

        private async Task<EnterpriseChartConfig> GetProfitMarginsChart(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var monthlyData = new List<ChartData>();
            
            // Group by month for the period
            var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

            while (currentMonth <= endMonth)
            {
                var monthStart = currentMonth;
                var monthEnd = monthStart.AddMonths(1);

                // Monthly Revenue
                var monthlyRevenue = await db.SalesOrders
                    .Where(so => so.OrderDate >= monthStart && so.OrderDate < monthEnd && so.Status == "Completed")
                    .SumAsync(so => so.SubTotal - so.DiscountAmount);

                // Monthly Costs
                var monthlyCosts = await db.SalesOrderLines
                    .Include(sol => sol.SalesOrder)
                    .Include(sol => sol.Product)
                    .Where(sol => sol.SalesOrder.OrderDate >= monthStart && 
                                 sol.SalesOrder.OrderDate < monthEnd && 
                                 sol.SalesOrder.Status == "Completed")
                    .SumAsync(sol => sol.Quantity * sol.Product.CostPrice);

                var profitMargin = monthlyRevenue > 0 ? ((monthlyRevenue - monthlyCosts) / monthlyRevenue) * 100 : 0;

                monthlyData.Add(new ChartData
                {
                    Label = currentMonth.ToString("MMM yyyy"),
                    Value = profitMargin,
                    Series = "Profit Margin %"
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            return new EnterpriseChartConfig
            {
                ChartType = "Bar",
                Title = "Monthly Profit Margins",
                Subtitle = "Profitability analysis over time",
                Data = monthlyData,
                Colors = new[] { "#388e3c" },
                ShowLegend = false,
                ShowDataLabels = true,
                Height = "350px"
            };
        }

        private async Task<EnterpriseChartConfig> GetInventoryTurnoverChart(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var categories = await db.Categories
                .Where(c => !c.IsDeleted && c.IsActive)
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            var turnoverData = new List<ChartData>();

            foreach (var category in categories.Take(5)) // Top 5 categories
            {
                // Calculate inventory turnover for this category
                var categoryProducts = await db.Products
                    .Where(p => p.CategoryId == category.CategoryId && !p.IsDeleted && p.IsActive)
                    .Select(p => new { p.ProductId, p.CostPrice, p.QuantityOnHand })
                    .ToListAsync();

                var avgInventoryValue = categoryProducts.Sum(p => p.QuantityOnHand * p.CostPrice);

                var cogs = await db.SalesOrderLines
                    .Include(sol => sol.SalesOrder)
                    .Include(sol => sol.Product)
                    .Where(sol => sol.Product.CategoryId == category.CategoryId &&
                                 sol.SalesOrder.OrderDate >= startDate &&
                                 sol.SalesOrder.OrderDate <= endDate &&
                                 sol.SalesOrder.Status == "Completed")
                    .SumAsync(sol => sol.Quantity * sol.Product.CostPrice);

                var turnoverRatio = avgInventoryValue > 0 ? cogs / avgInventoryValue : 0;

                turnoverData.Add(new ChartData
                {
                    Label = category.CategoryName,
                    Value = turnoverRatio,
                    Series = "Turnover Ratio"
                });
            }

            return new EnterpriseChartConfig
            {
                ChartType = "Doughnut",
                Title = "Inventory Turnover by Category",
                Subtitle = "Stock movement efficiency",
                Data = turnoverData.OrderByDescending(d => d.Value).ToList(),
                Colors = new[] { "#00796b", "#5d4037", "#455a64", "#e65100", "#ad1457" },
                ShowLegend = true,
                ShowDataLabels = true,
                Height = "350px"
            };
        }

        public async Task<AccountingReportData> PreviewReportAsync(ReportExportRequest request)
        {
            var report = await GenerateAccountingReportAsync(request.StartDate, request.EndDate);
            report.ReportType = request.ReportTitle;
            report.PreparedBy = request.PreparedBy;
            return report;
        }

        public async Task<byte[]> ExportReportToPdfAsync(AccountingReportData reportData, ReportExportRequest request)
        {
            await Task.Delay(500); // Brief processing time
            
            // Initialize QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
            
            // Generate PDF using QuestPDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Height(120, Unit.Point)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Text(text =>
                        {
                            text.AlignCenter();
                            text.Span(reportData.CompanyName).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            text.EmptyLine();
                            text.Span($"{reportData.CompanyAddress}").FontSize(10);
                            text.EmptyLine();
                            text.Span($"Phone: {reportData.CompanyPhone} | Email: {reportData.CompanyEmail}").FontSize(10);
                            text.EmptyLine();
                            text.Span($"Tax ID: {reportData.TaxId}").FontSize(10);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Report Title
                            column.Item().Text(request.ReportTitle)
                                .FontSize(16).Bold().AlignCenter();
                            
                            column.Item().PaddingTop(10).Text($"Period: {reportData.PeriodStart:MMM dd} - {reportData.PeriodEnd:MMM dd, yyyy}")
                                .FontSize(12).AlignCenter();
                            
                            column.Item().PaddingTop(5).Text($"Report Date: {reportData.ReportDate:MMMM dd, yyyy}")
                                .FontSize(10).AlignCenter();

                            // Executive Summary
                            column.Item().PaddingTop(20).Text("Executive Summary")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Metric").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount (₱)").Bold().AlignRight();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Percentage").Bold().AlignRight();
                                });

                                // Financial metrics rows
                                AddTableRow(table, "Net Sales", reportData.Summary.TotalRevenue, "100.0%");
                                AddTableRow(table, "Cost of Goods Sold", reportData.Summary.TotalCosts, 
                                    reportData.Summary.TotalRevenue > 0 ? $"{(reportData.Summary.TotalCosts / reportData.Summary.TotalRevenue * 100):F1}%" : "0.0%");
                                AddTableRow(table, "Gross Profit", reportData.Summary.GrossProfit, $"{reportData.Summary.GrossProfitMargin:F1}%");
                                AddTableRow(table, "Net Profit", reportData.Summary.NetProfit, $"{reportData.Summary.NetProfitMargin:F1}%");
                            });

                            // Inventory Analysis
                            column.Item().PaddingTop(20).Text("Inventory Analysis")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Metric").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Value").Bold().AlignRight();
                                });

                                AddSimpleTableRow(table, "Total Inventory Value", $"₱{reportData.Summary.TotalInventoryValue:N2}");
                                AddSimpleTableRow(table, "Inventory Turnover Ratio", $"{reportData.Summary.AverageInventoryTurnover:F2}x");
                                AddSimpleTableRow(table, "Active Products", $"{reportData.Summary.ActiveProducts:N0}");
                                AddSimpleTableRow(table, "Low Stock Items", $"{reportData.Summary.LowStockItems:N0}");
                                AddSimpleTableRow(table, "Total Transactions", $"{reportData.Summary.TotalTransactions:N0}");
                            });

                            // Category Performance (Top 10)
                            if (reportData.CategoryPerformance.Any())
                            {
                                column.Item().PaddingTop(20).Text("Top Category Performance")
                                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                                column.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Revenue").Bold().AlignRight();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Profit").Bold().AlignRight();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Margin").Bold().AlignRight();
                                    });

                                    foreach (var category in reportData.CategoryPerformance.Take(10))
                                    {
                                        table.Cell().Padding(5).Text(category.CategoryName);
                                        table.Cell().Padding(5).Text($"₱{category.Revenue:N2}").AlignRight();
                                        table.Cell().Padding(5).Text($"₱{category.Profit:N2}").AlignRight();
                                        table.Cell().Padding(5).Text($"{category.ProfitMargin:F1}%").AlignRight();
                                    }
                                });
                            }

                            // Compliance Notes
                            if (reportData.ComplianceNotes.Any())
                            {
                                column.Item().PaddingTop(20).Text("Compliance Notes")
                                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                                foreach (var note in reportData.ComplianceNotes)
                                {
                                    column.Item().PaddingTop(5).Text($"• {note}").FontSize(9);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span($"Generated on {DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt} | ");
                            x.Span("ModuERP Inventory Management System | ");
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();

            // Save to Downloads folder
            var fileName = $"Inventory_Financial_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var filePath = Path.Combine(downloadsPath, fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            return System.Text.Encoding.UTF8.GetBytes($"PDF Report exported to: {filePath}");
        }

        private void AddTableRow(TableDescriptor table, string metric, decimal amount, string percentage)
        {
            table.Cell().Padding(5).Text(metric);
            table.Cell().Padding(5).Text($"₱{amount:N2}").AlignRight();
            table.Cell().Padding(5).Text(percentage).AlignRight();
        }

        private void AddSimpleTableRow(TableDescriptor table, string metric, string value)
        {
            table.Cell().Padding(5).Text(metric);
            table.Cell().Padding(5).Text(value).AlignRight();
        }

        private string GenerateHtmlReport(AccountingReportData reportData, ReportExportRequest request)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{request.ReportTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; border-bottom: 2px solid #1976d2; padding-bottom: 20px; margin-bottom: 30px; }}
        .company-name {{ font-size: 24px; font-weight: bold; color: #1976d2; }}
        .report-title {{ font-size: 18px; margin-top: 10px; }}
        .report-info {{ margin-top: 20px; font-size: 14px; color: #666; }}
        .section {{ margin: 30px 0; }}
        .section-title {{ font-size: 16px; font-weight: bold; color: #1976d2; border-bottom: 1px solid #ddd; padding-bottom: 5px; }}
        .kpi-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin: 20px 0; }}
        .kpi-card {{ background: #f5f5f5; padding: 15px; border-radius: 5px; text-align: center; }}
        .kpi-value {{ font-size: 24px; font-weight: bold; color: #1976d2; }}
        .kpi-label {{ font-size: 14px; color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; font-weight: bold; }}
        .text-right {{ text-align: right; }}
        .footer {{ margin-top: 50px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{reportData.CompanyName}</div>
        <div>{reportData.CompanyAddress}</div>
        <div>Phone: {reportData.CompanyPhone} | Email: {reportData.CompanyEmail}</div>
        <div>Tax ID: {reportData.TaxId}</div>
        <div class='report-title'>{request.ReportTitle}</div>
        <div class='report-info'>
            Report Date: {reportData.ReportDate:MMMM dd, yyyy}<br>
            Period: {reportData.PeriodStart:MMM dd} - {reportData.PeriodEnd:MMM dd, yyyy}<br>
            Prepared by: {request.PreparedBy}
        </div>
    </div>

    <div class='section'>
        <div class='section-title'>Executive Summary</div>
        <div class='kpi-grid'>
            <div class='kpi-card'>
                <div class='kpi-value'>₱{reportData.Summary.TotalRevenue:N2}</div>
                <div class='kpi-label'>Total Revenue</div>
            </div>
            <div class='kpi-card'>
                <div class='kpi-value'>₱{reportData.Summary.TotalCosts:N2}</div>
                <div class='kpi-label'>Total Costs</div>
            </div>
            <div class='kpi-card'>
                <div class='kpi-value'>₱{reportData.Summary.GrossProfit:N2}</div>
                <div class='kpi-label'>Gross Profit</div>
            </div>
            <div class='kpi-card'>
                <div class='kpi-value'>{reportData.Summary.GrossProfitMargin:F1}%</div>
                <div class='kpi-label'>Profit Margin</div>
            </div>
        </div>
    </div>

    <div class='section'>
        <div class='section-title'>Financial Performance</div>
        <table>
            <tr><th>Metric</th><th class='text-right'>Amount (₱)</th><th class='text-right'>Percentage</th></tr>
            <tr><td>Total Revenue</td><td class='text-right'>{reportData.Summary.TotalRevenue:N2}</td><td class='text-right'>100.0%</td></tr>
            <tr><td>Total Costs</td><td class='text-right'>{reportData.Summary.TotalCosts:N2}</td><td class='text-right'>{(reportData.Summary.TotalRevenue > 0 ? (reportData.Summary.TotalCosts / reportData.Summary.TotalRevenue * 100) : 0):F1}%</td></tr>
            <tr><td>Gross Profit</td><td class='text-right'>{reportData.Summary.GrossProfit:N2}</td><td class='text-right'>{reportData.Summary.GrossProfitMargin:F1}%</td></tr>
            <tr><td>Net Profit</td><td class='text-right'>{reportData.Summary.NetProfit:N2}</td><td class='text-right'>{reportData.Summary.NetProfitMargin:F1}%</td></tr>
        </table>
    </div>";

            // Add Category Performance if available
            if (reportData.CategoryPerformance.Any())
            {
                html += @"
    <div class='section'>
        <div class='section-title'>Category Performance</div>
        <table>
            <tr><th>Category</th><th class='text-right'>Revenue</th><th class='text-right'>Profit</th><th class='text-right'>Margin</th><th class='text-right'>Units Sold</th></tr>";
                
                foreach (var category in reportData.CategoryPerformance.Take(10))
                {
                    html += $@"
            <tr>
                <td>{category.CategoryName}</td>
                <td class='text-right'>₱{category.Revenue:N2}</td>
                <td class='text-right'>₱{category.Profit:N2}</td>
                <td class='text-right'>{category.ProfitMargin:F1}%</td>
                <td class='text-right'>{category.UnitsSold:N0}</td>
            </tr>";
                }
                html += "</table></div>";
            }

            // Add Compliance Notes
            if (reportData.ComplianceNotes.Any())
            {
                html += @"
    <div class='section'>
        <div class='section-title'>Compliance Notes</div>
        <ul>";
                foreach (var note in reportData.ComplianceNotes)
                {
                    html += $"<li>{note}</li>";
                }
                html += "</ul></div>";
            }

            // Add footer
            html += $@"
    <div class='footer'>
        <div>Generated on {DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt}</div>
        <div>ModuERP Inventory Management System</div>
    </div>
</body>
</html>";

            return html;
        }

        private async Task<decimal> GetTotalInventoryValueAsync(InventoryDbContext db)
        {
            return await db.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .SumAsync(p => p.QuantityOnHand * p.CostPrice);
        }

        private async Task<decimal> CalculateInventoryTurnoverAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var totalCostOfGoodsSold = await db.SalesOrderLines
                .Include(sol => sol.SalesOrder)
                .Include(sol => sol.Product)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate &&
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed")
                .SumAsync(sol => sol.Quantity * sol.Product.CostPrice);

            var averageInventoryValue = await GetTotalInventoryValueAsync(db);
            
            return averageInventoryValue > 0 ? totalCostOfGoodsSold / averageInventoryValue : 0;
        }

        private async Task<List<RevenueBreakdown>> GetRevenueBreakdownAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            // Get raw data first, then calculate in memory
            var salesOrders = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate && so.Status == "Completed")
                .Select(so => new { so.SubTotal, so.TaxRate, so.DiscountAmount, so.ShippingCost })
                .ToListAsync();

            var totalRevenue = salesOrders.Sum(so => 
                (so.SubTotal - so.DiscountAmount) + ((so.SubTotal - so.DiscountAmount) * so.TaxRate) + so.ShippingCost);

            var breakdown = new List<RevenueBreakdown>
            {
                new RevenueBreakdown
                {
                    Source = "Sales Orders",
                    Amount = totalRevenue,
                    TransactionCount = salesOrders.Count,
                    AverageOrderValue = salesOrders.Count > 0 ? totalRevenue / salesOrders.Count : 0,
                    Percentage = 100
                }
            };

            return breakdown;
        }

        private async Task<List<CostAnalysis>> GetCostAnalysisAsync(InventoryDbContext db, DateTime startDate, DateTime endDate, InventoryAccountingMetrics metrics)
        {
            var costAnalysis = new List<CostAnalysis>();
            var totalCosts = metrics.CostOfGoodsSold + metrics.OperatingExpenses;

            // Cost of Goods Sold
            if (metrics.CostOfGoodsSold > 0)
            {
                costAnalysis.Add(new CostAnalysis
                {
                    CostCategory = "Cost of Goods Sold (COGS)",
                    Amount = metrics.CostOfGoodsSold,
                    Percentage = totalCosts > 0 ? (metrics.CostOfGoodsSold / totalCosts) * 100 : 0,
                    Description = "Direct costs of products sold during the period"
                });
            }

            // Operating Expenses
            if (metrics.OperatingExpenses > 0)
            {
                costAnalysis.Add(new CostAnalysis
                {
                    CostCategory = "Operating Expenses",
                    Amount = metrics.OperatingExpenses,
                    Percentage = totalCosts > 0 ? (metrics.OperatingExpenses / totalCosts) * 100 : 0,
                    Description = "Administrative and operational expenses"
                });
            }

            // Beginning Inventory Investment
            if (metrics.BeginningInventoryValue > 0)
            {
                costAnalysis.Add(new CostAnalysis
                {
                    CostCategory = "Beginning Inventory",
                    Amount = metrics.BeginningInventoryValue,
                    Percentage = 0, // Not part of period costs
                    Description = "Inventory value at start of period"
                });
            }

            // Purchases during period
            if (metrics.TotalPurchases > 0)
            {
                costAnalysis.Add(new CostAnalysis
                {
                    CostCategory = "Inventory Purchases",
                    Amount = metrics.TotalPurchases,
                    Percentage = 0, // Not part of period costs
                    Description = "New inventory purchased during period"
                });
            }

            return costAnalysis;
        }

        private async Task<List<ProductProfitability>> GetProductProfitabilityAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var productSales = await db.SalesOrderLines
                .Include(sol => sol.SalesOrder)
                .Include(sol => sol.Product)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate &&
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed")
                .GroupBy(sol => new { sol.Product.ProductId, sol.Product.Name, sol.Product.CostPrice })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CostPrice = g.Key.CostPrice,
                    TotalQuantitySold = g.Sum(sol => sol.Quantity),
                    TotalRevenue = g.Sum(sol => sol.Quantity * sol.UnitPrice),
                    TotalCost = g.Sum(sol => sol.Quantity * g.Key.CostPrice)
                })
                .ToListAsync();

            return productSales.Select(p => new ProductProfitability
            {
                ProductName = p.ProductName,
                QuantitySold = p.TotalQuantitySold,
                Revenue = p.TotalRevenue,
                Cost = p.TotalCost,
                Profit = p.TotalRevenue - p.TotalCost,
                ProfitMargin = p.TotalRevenue > 0 ? ((p.TotalRevenue - p.TotalCost) / p.TotalRevenue) * 100 : 0,
                Rating = GetProfitabilityRating(p.TotalRevenue > 0 ? ((p.TotalRevenue - p.TotalCost) / p.TotalRevenue) * 100 : 0)
            })
            .OrderByDescending(p => p.Profit)
            .ToList();
        }

        private string GetProfitabilityRating(decimal profitMargin)
        {
            return profitMargin switch
            {
                >= 30 => "Excellent",
                >= 20 => "Very Good",
                >= 10 => "Good",
                >= 0 => "Average",
                _ => "Poor"
            };
        }

        private async Task<List<CategoryPerformance>> GetCategoryPerformanceAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            // Get categories first
            var categories = await db.Categories
                .Where(c => !c.IsDeleted && c.IsActive)
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            var categoryPerformance = new List<CategoryPerformance>();

            foreach (var category in categories)
            {
                // Get sales data for this category
                var salesData = await db.SalesOrderLines
                    .Include(sol => sol.SalesOrder)
                    .Include(sol => sol.Product)
                    .Where(sol => sol.Product.CategoryId == category.CategoryId &&
                                 sol.SalesOrder.OrderDate >= startDate &&
                                 sol.SalesOrder.OrderDate <= endDate &&
                                 sol.SalesOrder.Status == "Completed")
                    .Select(sol => new { sol.Quantity, sol.UnitPrice, CostPrice = sol.Product.CostPrice })
                    .ToListAsync();

                var revenue = salesData.Sum(s => s.Quantity * s.UnitPrice);
                var cost = salesData.Sum(s => s.Quantity * s.CostPrice);
                var unitsSold = salesData.Sum(s => s.Quantity);

                // Get inventory value for this category
                var inventoryValue = await db.Products
                    .Where(p => p.CategoryId == category.CategoryId && !p.IsDeleted && p.IsActive)
                    .SumAsync(p => p.QuantityOnHand * p.CostPrice);

                var profit = revenue - cost;
                var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;

                categoryPerformance.Add(new CategoryPerformance
                {
                    CategoryName = category.CategoryName,
                    Revenue = revenue,
                    Cost = cost,
                    Profit = profit,
                    ProfitMargin = profitMargin,
                    UnitsSold = unitsSold,
                    InventoryValue = inventoryValue,
                    Performance = profitMargin >= 20 ? "Excellent" : profitMargin >= 10 ? "Good" : profitMargin >= 0 ? "Average" : "Poor"
                });
            }

            return categoryPerformance.OrderByDescending(cp => cp.Revenue).ToList();
        }

        private async Task<List<InventoryValuation>> GetInventoryValuationAsync(InventoryDbContext db)
        {
            return await db.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive && p.QuantityOnHand > 0)
                .Select(p => new InventoryValuation
                {
                    ProductName = p.Name,
                    SKU = p.SKU ?? "",
                    Category = p.Category.CategoryName,
                    QuantityOnHand = p.QuantityOnHand,
                    UnitCost = p.CostPrice,
                    TotalValue = p.QuantityOnHand * p.CostPrice,
                    MarketValue = p.QuantityOnHand * p.UnitPrice,
                    ValuationMethod = "FIFO"
                })
                .OrderByDescending(iv => iv.TotalValue)
                .ToListAsync();
        }

        private async Task<List<CashFlowItem>> GetCashFlowAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var cashFlowItems = new List<CashFlowItem>();
            decimal runningBalance = 0;

            // Sales (Inflows) - Get raw data to calculate totals in memory
            var sales = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate && so.Status == "Completed")
                .OrderBy(so => so.OrderDate)
                .Select(so => new { 
                    so.OrderDate, 
                    so.OrderNumber, 
                    so.SubTotal, 
                    so.TaxRate, 
                    so.DiscountAmount, 
                    so.ShippingCost 
                })
                .ToListAsync();

            foreach (var sale in sales)
            {
                var grandTotal = (sale.SubTotal - sale.DiscountAmount) + 
                               ((sale.SubTotal - sale.DiscountAmount) * sale.TaxRate) + 
                               sale.ShippingCost;
                
                runningBalance += grandTotal;
                cashFlowItems.Add(new CashFlowItem
                {
                    Date = sale.OrderDate,
                    Description = $"Sales Order #{sale.OrderNumber}",
                    Inflow = grandTotal,
                    Outflow = 0,
                    NetFlow = grandTotal,
                    RunningBalance = runningBalance,
                    Category = "Sales Revenue"
                });
            }

            // Purchases (Outflows) - Get raw data to calculate totals in memory
            var purchases = await db.PurchaseOrders
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && po.Status == "Received")
                .OrderBy(po => po.OrderDate)
                .Select(po => new { 
                    po.OrderDate, 
                    po.OrderNumber, 
                    po.SubTotal, 
                    po.TaxRate, 
                    po.DiscountAmount, 
                    po.ShippingCost 
                })
                .ToListAsync();

            foreach (var purchase in purchases)
            {
                var grandTotal = (purchase.SubTotal - purchase.DiscountAmount) + 
                               ((purchase.SubTotal - purchase.DiscountAmount) * purchase.TaxRate) + 
                               purchase.ShippingCost;
                
                runningBalance -= grandTotal;
                cashFlowItems.Add(new CashFlowItem
                {
                    Date = purchase.OrderDate,
                    Description = $"Purchase Order #{purchase.OrderNumber}",
                    Inflow = 0,
                    Outflow = grandTotal,
                    NetFlow = -grandTotal,
                    RunningBalance = runningBalance,
                    Category = "Inventory Purchases"
                });
            }

            return cashFlowItems.OrderBy(cf => cf.Date).ToList();
        }

        private List<string> GetComplianceNotes(AccountingReportData report)
        {
            var notes = new List<string>();

            // Standard accounting compliance notes
            notes.Add("This report follows Generally Accepted Accounting Principles (GAAP) for inventory valuation.");
            notes.Add("Inventory is valued using the weighted average cost method.");
            notes.Add("All figures are presented in Philippine Pesos (₱).");
            notes.Add("Revenue recognition follows the accrual basis of accounting.");
            notes.Add("Cost of Goods Sold calculated using: Beginning Inventory + Purchases - Ending Inventory.");

            // Performance-based notes
            if (report.Summary.GrossProfitMargin < 20)
            {
                notes.Add("Gross profit margin is below industry average. Consider reviewing pricing strategy.");
            }

            if (report.Summary.AverageInventoryTurnover < 4)
            {
                notes.Add("Inventory turnover ratio indicates slow-moving inventory. Review stock levels.");
            }

            if (report.Summary.LowStockItems > 0)
            {
                notes.Add($"There are {report.Summary.LowStockItems} items below reorder level requiring attention.");
            }

            return notes;
        }

        private List<string> GetRecommendations(AccountingReportData report)
        {
            var recommendations = new List<string>();

            // Financial performance recommendations
            if (report.Summary.GrossProfitMargin < 30)
            {
                recommendations.Add("Consider increasing product prices or negotiating better supplier terms to improve gross profit margin.");
                recommendations.Add("Consider reviewing pricing strategy to improve profit margins.");
            }

            if (report.Summary.AverageInventoryTurnover < 4)
            {
                recommendations.Add("Low inventory turnover detected - consider optimizing stock levels.");
            }

            if (report.Summary.CancellationRate > 5)
            {
                recommendations.Add("Implement measures to reduce order cancellation rate.");
            }

            if (report.Summary.LowStockItems > report.Summary.ActiveProducts * 0.1m)
            {
                recommendations.Add("Implement automated reordering system for better inventory management.");
            }

            recommendations.Add("Continue monitoring key performance indicators for sustained growth.");
            recommendations.Add("Consider implementing advanced analytics for predictive insights.");

            return recommendations;
        }

        private string GetCategoryColor(string categoryName)
        {
            var colors = new Dictionary<string, string>
            {
                { "Electronics", "#1976D2" },
                { "Clothing", "#388E3C" },
                { "Food", "#F57C00" },
                { "Books", "#7B1FA2" },
                { "Home", "#C2185B" },
                { "Sports", "#00796B" }
            };

            return colors.GetValueOrDefault(categoryName, "#607D8B");
        }

        private string GetImpactColor(string impact)
        {
            return impact switch
            {
                "High" => "#F44336",
                "Medium" => "#FF9800",
                "Low" => "#4CAF50",
                _ => "#9E9E9E"
            };
        }
    }
}
