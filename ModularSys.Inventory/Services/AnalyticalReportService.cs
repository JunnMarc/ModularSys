using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Services
{
    public class AnalyticalReportService : IAnalyticalReportService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AnalyticalReportService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // 1. Inventory Valuation Report
        public async Task<InventoryValuationReport> GenerateInventoryValuationReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var products = await db.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive)
                .ToListAsync();

            var report = new InventoryValuationReport
            {
                ReportDate = request.EndDate,
                ValuationMethod = "Weighted Average"
            };

            // Calculate inventory items
            report.Items = products.Select(p => new InventoryValuationItem
            {
                ProductId = p.ProductId.ToString(),
                ProductName = p.Name,
                SKU = p.SKU,
                Category = p.Category?.CategoryName ?? "Uncategorized",
                QuantityOnHand = p.QuantityOnHand,
                UnitCost = p.CostPrice,
                TotalValue = p.QuantityOnHand * p.CostPrice,
                LastUpdated = p.UpdatedAt ?? p.CreatedAt ?? DateTime.UtcNow,
                ValuationMethod = "Weighted Average"
            }).ToList();

            // Calculate totals
            report.TotalInventoryValue = report.Items.Sum(i => i.TotalValue);
            report.EndingInventoryValue = report.TotalInventoryValue;

            // Category breakdown
            report.CategoryBreakdown = report.Items
                .GroupBy(i => i.Category)
                .Select(g => new CategoryValuation
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalQuantity = g.Sum(i => i.QuantityOnHand),
                    TotalValue = g.Sum(i => i.TotalValue),
                    PercentageOfTotal = report.TotalInventoryValue > 0 ? (g.Sum(i => i.TotalValue) / report.TotalInventoryValue) * 100 : 0,
                    AverageCost = g.Average(i => i.UnitCost)
                }).OrderByDescending(c => c.TotalValue).ToList();

            return report;
        }

        // 2. COGS Report
        public async Task<COGSReport> GenerateCOGSReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var salesOrderLines = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .ThenInclude(p => p.Category)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= request.StartDate &&
                             sol.SalesOrder.OrderDate <= request.EndDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted)
                .ToListAsync();

            var report = new COGSReport
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                GrossSales = salesOrderLines.Sum(sol => sol.LineTotal),
                NetSales = salesOrderLines.Sum(sol => sol.LineTotal),
                CostOfGoodsSold = salesOrderLines.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0))
            };

            report.GrossProfit = report.NetSales - report.CostOfGoodsSold;
            report.GrossProfitMargin = report.NetSales > 0 ? (report.GrossProfit / report.NetSales) * 100 : 0;

            // Category breakdown
            report.CategoryBreakdown = salesOrderLines
                .GroupBy(sol => sol.Product?.Category?.CategoryName ?? "Uncategorized")
                .Select(g => new COGSByCategory
                {
                    CategoryName = g.Key,
                    COGS = g.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0)),
                    Revenue = g.Sum(sol => sol.LineTotal),
                    GrossProfit = g.Sum(sol => sol.LineTotal) - g.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0)),
                    GrossProfitMargin = g.Sum(sol => sol.LineTotal) > 0 ? 
                        ((g.Sum(sol => sol.LineTotal) - g.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0))) / g.Sum(sol => sol.LineTotal)) * 100 : 0,
                    PercentageOfTotalCOGS = report.CostOfGoodsSold > 0 ? (g.Sum(sol => sol.Quantity * (sol.Product?.CostPrice ?? 0)) / report.CostOfGoodsSold) * 100 : 0
                }).OrderByDescending(c => c.COGS).ToList();

            return report;
        }

        // 3. Inventory Turnover Report
        public async Task<InventoryTurnoverReport> GenerateInventoryTurnoverReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var products = await db.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive)
                .ToListAsync();

            var salesData = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= request.StartDate &&
                             sol.SalesOrder.OrderDate <= request.EndDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted)
                .GroupBy(sol => sol.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalCOGS = g.Sum(sol => sol.Quantity * (sol.Product.CostPrice)),
                    UnitsSold = g.Sum(sol => sol.Quantity)
                })
                .ToListAsync();

            // Calculate actual days in the date range
            var daysInPeriod = (request.EndDate - request.StartDate).Days + 1;
            
            var report = new InventoryTurnoverReport
            {
                ReportDate = request.EndDate,
                AnnualCOGS = salesData.Sum(s => s.TotalCOGS),
                AverageInventoryValue = products.Sum(p => p.QuantityOnHand * p.CostPrice)
            };

            // Calculate period turnover ratio and annualize it for comparison
            var periodTurnoverRatio = report.AverageInventoryValue > 0 ? report.AnnualCOGS / report.AverageInventoryValue : 0;
            report.OverallTurnoverRatio = daysInPeriod > 0 ? (365.0m / daysInPeriod) * periodTurnoverRatio : 0;
            
            // Days sales in inventory should be realistic for the period
            report.DaysSalesInInventory = periodTurnoverRatio > 0 ? Math.Min(daysInPeriod / periodTurnoverRatio, daysInPeriod * 2) : daysInPeriod;

            // Product turnover analysis
            report.ProductTurnover = products.Select(p =>
            {
                var sales = salesData.FirstOrDefault(s => s.ProductId == p.ProductId);
                var currentInventoryValue = p.QuantityOnHand * p.CostPrice;
                var cogs = sales?.TotalCOGS ?? 0;
                var unitsSold = sales?.UnitsSold ?? 0;
                
                // Calculate turnover ratio: if we sold units, calculate how many times we turned inventory
                // For short periods, we need to annualize the ratio for proper classification
                var periodTurnoverRatio = currentInventoryValue > 0 ? cogs / currentInventoryValue : 0;
                
                // Annualize the turnover ratio for proper classification (365 days / period days * period ratio)
                var annualizedTurnoverRatio = daysInPeriod > 0 ? (365.0m / daysInPeriod) * periodTurnoverRatio : 0;
                
                // Days sales in inventory should be realistic for the period
                var daysInInventory = periodTurnoverRatio > 0 ? daysInPeriod / periodTurnoverRatio : daysInPeriod;
                
                // If no sales in period, set to maximum days
                if (unitsSold == 0)
                {
                    daysInInventory = daysInPeriod * 10; // Indicate very slow movement
                    annualizedTurnoverRatio = 0;
                }

                return new ProductTurnover
                {
                    ProductId = p.ProductId.ToString(),
                    ProductName = p.Name,
                    SKU = p.SKU,
                    Category = p.Category?.CategoryName ?? "Uncategorized",
                    TurnoverRatio = annualizedTurnoverRatio, // Use annualized for classification
                    AverageInventory = currentInventoryValue,
                    COGS = cogs,
                    DaysSalesInInventory = Math.Min(daysInInventory, daysInPeriod * 5), // Cap at reasonable maximum
                    TurnoverClassification = GetTurnoverClassification(annualizedTurnoverRatio, daysInPeriod),
                    RecommendedAction = GetTurnoverRecommendation(annualizedTurnoverRatio, unitsSold, daysInPeriod)
                };
            }).OrderByDescending(pt => pt.TurnoverRatio).ToList();

            return report;
        }

        // 4. Stock Aging Report
        public async Task<StockAgingReport> GenerateStockAgingReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var products = await db.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive && p.QuantityOnHand > 0)
                .ToListAsync();

            var report = new StockAgingReport
            {
                ReportDate = request.EndDate
            };

            // Get last movement dates (simplified - using created date)
            report.ProductAging = products.Select(p =>
            {
                var daysInStock = (int)((request.EndDate - p.CreatedAt)?.TotalDays ?? 0);
                return new ProductAging
                {
                    ProductId = p.ProductId.ToString(),
                    ProductName = p.Name,
                    SKU = p.SKU,
                    Category = p.Category?.CategoryName ?? "Uncategorized",
                    QuantityOnHand = p.QuantityOnHand,
                    UnitCost = p.CostPrice,
                    TotalValue = p.QuantityOnHand * p.CostPrice,
                    LastMovementDate = p.CreatedAt ?? DateTime.UtcNow,
                    DaysInStock = daysInStock,
                    AgingBucket = GetAgingBucket(daysInStock),
                    RiskLevel = GetRiskLevel(daysInStock),
                    ExpiryDate = p.ExpiryDate,
                    RecommendedAction = GetAgingRecommendation(daysInStock)
                };
            }).ToList();

            // Create aging buckets
            report.AgingBuckets = new List<AgingBucket>
            {
                CreateAgingBucket("0-30 days", 0, 30, report.ProductAging),
                CreateAgingBucket("31-60 days", 31, 60, report.ProductAging),
                CreateAgingBucket("61-90 days", 61, 90, report.ProductAging),
                CreateAgingBucket("91-180 days", 91, 180, report.ProductAging),
                CreateAgingBucket("Over 180 days", 181, int.MaxValue, report.ProductAging)
            };

            return report;
        }

        // 5. Product Profitability Report
        public async Task<ProductProfitabilityReport> GenerateProductProfitabilityReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var salesOrderLines = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .ThenInclude(p => p.Category)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= request.StartDate &&
                             sol.SalesOrder.OrderDate <= request.EndDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted)
                .ToListAsync();

            var productProfitability = salesOrderLines
                .GroupBy(sol => sol.ProductId)
                .Select(g =>
                {
                    var product = g.First().Product;
                    var revenue = g.Sum(sol => sol.LineTotal);
                    var cogs = g.Sum(sol => sol.Quantity * (product?.CostPrice ?? 0));
                    var grossProfit = revenue - cogs;
                    var grossProfitMargin = revenue > 0 ? (grossProfit / revenue) * 100 : 0;

                    return new ProductProfitabilityDetail
                    {
                        ProductId = g.Key.ToString(),
                        ProductName = product?.Name ?? "Unknown",
                        SKU = product?.SKU ?? "",
                        Category = product?.Category?.CategoryName ?? "Uncategorized",
                        UnitsSold = g.Sum(sol => sol.Quantity),
                        Revenue = revenue,
                        COGS = cogs,
                        GrossProfit = grossProfit,
                        GrossProfitMargin = grossProfitMargin,
                        AverageSellingPrice = g.Average(sol => sol.UnitPrice),
                        AverageCost = product?.CostPrice ?? 0,
                        ContributionMargin = grossProfitMargin,
                        ProfitabilityRating = GetProfitabilityRating(grossProfitMargin),
                        ROI = (product?.CostPrice ?? 0) > 0 ? (grossProfit / (product.CostPrice * g.Sum(sol => sol.Quantity))) * 100 : 0
                    };
                })
                .OrderByDescending(p => p.GrossProfit)
                .ToList();

            // Add ranking
            for (int i = 0; i < productProfitability.Count; i++)
            {
                productProfitability[i].Ranking = i + 1;
            }

            var report = new ProductProfitabilityReport
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Products = productProfitability
            };

            // Category profitability
            report.Categories = productProfitability
                .GroupBy(p => p.Category)
                .Select(g => new CategoryProfitability
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(p => p.Revenue),
                    COGS = g.Sum(p => p.COGS),
                    GrossProfit = g.Sum(p => p.GrossProfit),
                    GrossProfitMargin = g.Sum(p => p.Revenue) > 0 ? (g.Sum(p => p.GrossProfit) / g.Sum(p => p.Revenue)) * 100 : 0,
                    ProductCount = g.Count(),
                    UnitsSold = g.Sum(p => p.UnitsSold),
                    PercentageOfTotalRevenue = productProfitability.Sum(p => p.Revenue) > 0 ? (g.Sum(p => p.Revenue) / productProfitability.Sum(p => p.Revenue)) * 100 : 0
                })
                .OrderByDescending(c => c.GrossProfit)
                .ToList();

            return report;
        }

        // Helper methods
        private string GetTurnoverClassification(decimal annualizedTurnoverRatio, int daysInPeriod)
        {
            // For short periods, be more lenient with classification
            if (daysInPeriod <= 30) // Less than a month
            {
                return annualizedTurnoverRatio switch
                {
                    >= 4 => "Fast",
                    >= 2 => "Medium", 
                    >= 0.5m => "Slow",
                    _ => "Very Slow"
                };
            }
            else // Longer periods use standard thresholds
            {
                return annualizedTurnoverRatio switch
                {
                    >= 6 => "Fast",
                    >= 3 => "Medium",
                    >= 1 => "Slow",
                    _ => "Very Slow"
                };
            }
        }

        private string GetTurnoverRecommendation(decimal annualizedTurnoverRatio, int unitsSold, int daysInPeriod)
        {
            if (unitsSold == 0)
            {
                return $"No sales in {daysInPeriod} days - review pricing and marketing";
            }

            if (daysInPeriod <= 30) // Short period recommendations
            {
                return annualizedTurnoverRatio switch
                {
                    >= 4 => "Good sales velocity - monitor stock levels",
                    >= 2 => "Moderate sales - track trends",
                    >= 0.5m => "Slow sales - consider promotion",
                    _ => "Very slow - review product viability"
                };
            }
            else // Standard recommendations
            {
                return annualizedTurnoverRatio switch
                {
                    >= 6 => "Excellent performance - maintain stock levels",
                    >= 3 => "Good performance - monitor regularly", 
                    >= 1 => "Slow moving - consider promotion or reduction",
                    _ => "Very slow - consider discontinuation"
                };
            }
        }

        private string GetAgingBucket(int daysInStock)
        {
            return daysInStock switch
            {
                <= 30 => "0-30 days",
                <= 60 => "31-60 days",
                <= 90 => "61-90 days",
                <= 180 => "91-180 days",
                _ => "Over 180 days"
            };
        }

        private string GetRiskLevel(int daysInStock)
        {
            return daysInStock switch
            {
                <= 30 => "Low",
                <= 60 => "Medium",
                <= 90 => "High",
                _ => "Critical"
            };
        }

        private string GetAgingRecommendation(int daysInStock)
        {
            return daysInStock switch
            {
                <= 30 => "Normal - no action required",
                <= 60 => "Monitor closely",
                <= 90 => "Consider promotion or discount",
                <= 180 => "Urgent action required - deep discount",
                _ => "Consider write-off or liquidation"
            };
        }

        private string GetProfitabilityRating(decimal grossProfitMargin)
        {
            return grossProfitMargin switch
            {
                >= 50 => "Excellent",
                >= 30 => "Good",
                >= 15 => "Fair",
                _ => "Poor"
            };
        }

        private AgingBucket CreateAgingBucket(string bucketName, int daysFrom, int daysTo, List<ProductAging> productAging)
        {
            var productsInBucket = productAging.Where(p => p.DaysInStock >= daysFrom && 
                (daysTo == int.MaxValue || p.DaysInStock <= daysTo)).ToList();

            var totalValue = productsInBucket.Sum(p => p.TotalValue);
            var totalInventoryValue = productAging.Sum(p => p.TotalValue);

            return new AgingBucket
            {
                BucketName = bucketName,
                DaysFrom = daysFrom,
                DaysTo = daysTo,
                ProductCount = productsInBucket.Count,
                TotalValue = totalValue,
                PercentageOfTotal = totalInventoryValue > 0 ? (totalValue / totalInventoryValue) * 100 : 0
            };
        }

        // ERP Chart Generation
        public async Task<List<ERPChartConfig>> GenerateERPChartsAsync(AnalyticalReportRequest request)
        {
            var charts = new List<ERPChartConfig>();

            // Add inventory valuation pie chart
            var valuationReport = await GenerateInventoryValuationReportAsync(request);
            charts.Add(CreateInventoryValuationChart(valuationReport));

            // Add turnover analysis chart
            var turnoverReport = await GenerateInventoryTurnoverReportAsync(request);
            charts.Add(CreateTurnoverAnalysisChart(turnoverReport));

            // Add profitability chart
            var profitabilityReport = await GenerateProductProfitabilityReportAsync(request);
            charts.Add(CreateProfitabilityChart(profitabilityReport));

            return charts;
        }

        private ERPChartConfig CreateInventoryValuationChart(InventoryValuationReport report)
        {
            return new ERPChartConfig
            {
                Title = "Inventory Valuation by Category",
                Subtitle = $"Total Value: {report.TotalInventoryValue:C}",
                ChartType = ERPChartType.Pie,
                Series = new List<ERPChartSeries>
                {
                    new ERPChartSeries
                    {
                        Name = "Inventory Value",
                        Data = report.CategoryBreakdown.Take(10).Select(c => new ERPChartDataPoint
                        {
                            Label = c.CategoryName,
                            Value = c.TotalValue
                        }).ToList()
                    }
                },
                Options = new ERPChartOptions
                {
                    Tooltip = new ERPChartTooltip
                    {
                        Format = "{point.name}: {point.y:C} ({point.percentage:.1f}%)"
                    }
                }
            };
        }

        private ERPChartConfig CreateTurnoverAnalysisChart(InventoryTurnoverReport report)
        {
            return new ERPChartConfig
            {
                Title = "Inventory Turnover Analysis",
                Subtitle = "Top 15 Products by Turnover Ratio",
                ChartType = ERPChartType.Column,
                Series = new List<ERPChartSeries>
                {
                    new ERPChartSeries
                    {
                        Name = "Turnover Ratio",
                        Data = report.ProductTurnover.Take(15).Select(p => new ERPChartDataPoint
                        {
                            Label = p.ProductName,
                            Value = p.TurnoverRatio
                        }).ToList()
                    }
                },
                Options = new ERPChartOptions
                {
                    YAxis = new ERPChartAxis
                    {
                        Title = "Turnover Ratio",
                        Format = "{value:.1f}"
                    }
                }
            };
        }

        private ERPChartConfig CreateProfitabilityChart(ProductProfitabilityReport report)
        {
            return new ERPChartConfig
            {
                Title = "Product Profitability Analysis",
                Subtitle = "Revenue vs Gross Profit - Top 15 Products",
                ChartType = ERPChartType.Combo,
                Series = new List<ERPChartSeries>
                {
                    new ERPChartSeries
                    {
                        Name = "Revenue",
                        Type = ERPChartType.Column,
                        Data = report.Products.Take(15).Select(p => new ERPChartDataPoint
                        {
                            Label = p.ProductName,
                            Value = p.Revenue
                        }).ToList()
                    },
                    new ERPChartSeries
                    {
                        Name = "Gross Profit Margin (%)",
                        Type = ERPChartType.Line,
                        YAxis = "secondary",
                        Data = report.Products.Take(15).Select(p => new ERPChartDataPoint
                        {
                            Label = p.ProductName,
                            Value = p.GrossProfitMargin
                        }).ToList()
                    }
                },
                Options = new ERPChartOptions
                {
                    YAxis = new ERPChartAxis
                    {
                        Title = "Revenue",
                        Format = "{value:C}"
                    },
                    SecondaryYAxis = new ERPChartAxis
                    {
                        Title = "Profit Margin (%)",
                        Format = "{value:.1f}%",
                        Position = "right"
                    }
                }
            };
        }

        // Crystal Reports Integration Methods (Implementation placeholders)
        public async Task<byte[]> GenerateCrystalReportAsync(AnalyticalReportRequest request)
        {
            // Generate the report data first
            object reportData = request.ReportType switch
            {
                AnalyticalReportType.InventoryValuation => await GenerateInventoryValuationReportAsync(request),
                AnalyticalReportType.CostOfGoodsSold => await GenerateCOGSReportAsync(request),
                AnalyticalReportType.InventoryTurnover => await GenerateInventoryTurnoverReportAsync(request),
                AnalyticalReportType.StockAging => await GenerateStockAgingReportAsync(request),
                AnalyticalReportType.ProfitabilityByProduct => await GenerateProductProfitabilityReportAsync(request),
                AnalyticalReportType.ProfitAndLossStatement => await GenerateProfitAndLossReportAsync(request),
                _ => throw new ArgumentException($"Unsupported report type: {request.ReportType}")
            };

            // Generate PDF using QuestPDF
            return PdfReportService.GenerateAnalyticalReportPdf(request, reportData);
        }

        public async Task<List<CrystalReportDefinition>> GetAvailableCrystalReportsAsync()
        {
            // TODO: Return available Crystal Reports templates
            await Task.Delay(100); // Placeholder for async operation
            
            return new List<CrystalReportDefinition>
            {
                new CrystalReportDefinition
                {
                    ReportName = "Inventory Valuation Report",
                    ReportPath = "/Reports/InventoryValuation.rpt",
                    Description = "Comprehensive inventory valuation with category breakdown",
                    ReportType = AnalyticalReportType.InventoryValuation,
                    SupportedFormats = new List<string> { "PDF", "Excel", "Word" }
                },
                new CrystalReportDefinition
                {
                    ReportName = "COGS Analysis Report",
                    ReportPath = "/Reports/COGSAnalysis.rpt",
                    Description = "Cost of Goods Sold analysis with profitability metrics",
                    ReportType = AnalyticalReportType.CostOfGoodsSold,
                    SupportedFormats = new List<string> { "PDF", "Excel" }
                },
                new CrystalReportDefinition
                {
                    ReportName = "Inventory Turnover Report",
                    ReportPath = "/Reports/InventoryTurnover.rpt",
                    Description = "Inventory turnover analysis with performance recommendations",
                    ReportType = AnalyticalReportType.InventoryTurnover,
                    SupportedFormats = new List<string> { "PDF", "Excel", "Word" }
                },
                new CrystalReportDefinition
                {
                    ReportName = "Stock Aging Report",
                    ReportPath = "/Reports/StockAging.rpt",
                    Description = "Stock aging analysis with risk assessment",
                    ReportType = AnalyticalReportType.StockAging,
                    SupportedFormats = new List<string> { "PDF", "Excel" }
                },
                new CrystalReportDefinition
                {
                    ReportName = "Product Profitability Report",
                    ReportPath = "/Reports/ProductProfitability.rpt",
                    Description = "Detailed product profitability analysis with rankings",
                    ReportType = AnalyticalReportType.ProfitabilityByProduct,
                    SupportedFormats = new List<string> { "PDF", "Excel", "Word" }
                },
                new CrystalReportDefinition
                {
                    ReportName = "Profit & Loss Statement",
                    ReportPath = "/Reports/ProfitAndLoss.rpt",
                    Description = "Comprehensive P&L statement showing business profitability",
                    ReportType = AnalyticalReportType.ProfitAndLossStatement,
                    SupportedFormats = new List<string> { "PDF", "Excel", "Word" }
                }
            };
        }

        // Comprehensive Profit & Loss Report Generation
        public async Task<ProfitAndLossReport> GenerateProfitAndLossReportAsync(AnalyticalReportRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var report = new ProfitAndLossReport
            {
                ReportDate = DateTime.Now,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                CompanyName = "ModularSys Enterprise"
            };

            // Calculate Revenue Section
            await CalculateRevenueSection(context, report, request);

            // Calculate COGS Section
            await CalculateCOGSSection(context, report, request);

            // Calculate Operating Expenses Section
            await CalculateOperatingExpensesSection(context, report, request);

            // Generate Monthly Breakdown
            await GenerateMonthlyProfitLossBreakdown(context, report, request);

            // Generate Business Insights and Recommendations
            GenerateBusinessInsights(report);

            return report;
        }

        private async Task CalculateRevenueSection(InventoryDbContext context, ProfitAndLossReport report, AnalyticalReportRequest request)
        {
            // Get all sales orders in the period
            var salesOrders = await context.SalesOrders
                .Where(so => !so.IsDeleted && 
                           so.OrderDate >= request.StartDate && 
                           so.OrderDate <= request.EndDate &&
                           so.Status != "Cancelled")
                .Include(so => so.Lines)
                .ThenInclude(sol => sol.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            // Calculate gross sales from actual data
            report.Revenue.GrossSales = salesOrders.Sum(so => so.SubTotal);

            // Calculate actual returns from cancelled orders
            var cancelledOrders = await context.SalesOrders
                .Where(so => !so.IsDeleted && 
                           so.OrderDate >= request.StartDate && 
                           so.OrderDate <= request.EndDate &&
                           so.Status == "Cancelled")
                .ToListAsync();
            
            report.Revenue.SalesReturns = cancelledOrders.Sum(so => so.SubTotal);

            // Calculate actual discounts from sales orders
            report.Revenue.SalesDiscounts = salesOrders.Sum(so => so.DiscountAmount);

            // Calculate revenue by category using actual data
            var categoryRevenue = salesOrders
                .GroupBy(so => so.Lines.FirstOrDefault()?.Product.Category?.CategoryName ?? "Uncategorized")
                .Select(g => new RevenueByCategory
                {
                    CategoryName = g.Key,
                    GrossSales = g.Sum(so => so.SubTotal),
                    Returns = 0, // Will be calculated from cancelled orders by category if needed
                    Discounts = g.Sum(so => so.DiscountAmount)
                })
                .ToList();

            // Calculate percentages
            foreach (var category in categoryRevenue)
            {
                category.PercentageOfTotal = report.Revenue.NetRevenue > 0 
                    ? (category.NetRevenue / report.Revenue.NetRevenue) * 100 
                    : 0;
            }

            report.Revenue.CategoryBreakdown = categoryRevenue;
        }

        private async Task CalculateCOGSSection(InventoryDbContext context, ProfitAndLossReport report, AnalyticalReportRequest request)
        {
            // Get inventory transactions for the period
            var inventoryTransactions = await context.InventoryTransactions
                .Where(it => !it.IsDeleted && 
                           it.TransactionDate >= request.StartDate && 
                           it.TransactionDate <= request.EndDate)
                .Include(it => it.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            // Calculate beginning inventory (inventory value at start of period)
            var beginningInventoryTransactions = await context.InventoryTransactions
                .Where(it => !it.IsDeleted && it.TransactionDate < request.StartDate)
                .Include(it => it.Product)
                .ToListAsync();

            report.CostOfGoodsSold.BeginningInventory = beginningInventoryTransactions
                .GroupBy(it => it.ProductId)
                .Sum(g => g.Sum(it => it.TransactionType == "Purchase" || it.TransactionType == "Adjustment_In" 
                    ? it.QuantityChange * it.UnitCost 
                    : -(it.QuantityChange * it.UnitCost)));

            // Calculate purchases during the period
            report.CostOfGoodsSold.Purchases = inventoryTransactions
                .Where(it => it.TransactionType == "Purchase")
                .Sum(it => it.QuantityChange * it.UnitCost);

            // For retail/inventory businesses, direct labor and manufacturing overhead are typically zero
            // These would only apply to manufacturing businesses
            report.CostOfGoodsSold.DirectLabor = 0m;
            report.CostOfGoodsSold.ManufacturingOverhead = 0m;

            // Calculate ending inventory (current inventory value)
            var currentInventory = await context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .ToListAsync();

            report.CostOfGoodsSold.EndingInventory = currentInventory
                .Sum(p => p.QuantityOnHand * p.CostPrice);

            // Calculate COGS by category
            var cogsByCategory = inventoryTransactions
                .Where(it => it.TransactionType == "Sale")
                .GroupBy(it => it.Product.Category?.CategoryName ?? "Uncategorized")
                .Select(g => new COGSByCategory
                {
                    CategoryName = g.Key,
                    COGS = g.Sum(it => it.QuantityChange * it.UnitCost),
                    Revenue = g.Sum(it => it.QuantityChange * it.Amount / it.QuantityChange), // Amount per unit
                    GrossProfit = g.Sum(it => it.QuantityChange * (it.Amount / it.QuantityChange - it.UnitCost)),
                    GrossProfitMargin = g.Sum(it => it.QuantityChange * it.Amount / it.QuantityChange) > 0 
                        ? (g.Sum(it => it.QuantityChange * (it.Amount / it.QuantityChange - it.UnitCost)) / g.Sum(it => it.QuantityChange * it.Amount / it.QuantityChange)) * 100 
                        : 0,
                    PercentageOfTotalCOGS = 0 // Will be calculated after total COGS is known
                })
                .ToList();

            // Calculate percentages of total COGS
            var totalCOGS = report.CostOfGoodsSold.TotalCOGS;
            foreach (var category in cogsByCategory)
            {
                category.PercentageOfTotalCOGS = totalCOGS > 0 ? (category.COGS / totalCOGS) * 100 : 0;
            }

            report.CostOfGoodsSold.CategoryBreakdown = cogsByCategory;
        }

        private async Task CalculateOperatingExpensesSection(InventoryDbContext context, ProfitAndLossReport report, AnalyticalReportRequest request)
        {   
            // Calculate actual shipping costs from orders (real expense data)
            var totalShippingCosts = await context.SalesOrders
                .Where(so => !so.IsDeleted && 
                           so.OrderDate >= request.StartDate && 
                           so.OrderDate <= request.EndDate &&
                           so.Status != "Cancelled")
                .SumAsync(so => so.ShippingCost);

            // Calculate actual tax costs from orders (real expense data)
            var totalTaxCosts = await context.SalesOrders
                .Where(so => !so.IsDeleted && 
                           so.OrderDate >= request.StartDate && 
                           so.OrderDate <= request.EndDate &&
                           so.Status != "Cancelled")
                .SumAsync(so => (so.SubTotal - so.DiscountAmount) * so.TaxRate);

            var netRevenue = report.Revenue.NetRevenue;
            
            // Tech retail business operating expenses (Philippines small business benchmarks)
            report.OperatingExpenses.SalariesAndWages = netRevenue * 0.12m; // 12% - small tech retail staff
            report.OperatingExpenses.Rent = netRevenue * 0.08m; // 8% - mall/commercial space rent (higher in PH)
            report.OperatingExpenses.Utilities = netRevenue * 0.025m; // 2.5% - electricity, internet, AC for tech products
            report.OperatingExpenses.Marketing = netRevenue * 0.04m; // 4% - online ads, social media, promos
            report.OperatingExpenses.Insurance = netRevenue * 0.015m; // 1.5% - product insurance, liability
            report.OperatingExpenses.Depreciation = netRevenue * 0.015m; // 1.5% - display equipment, POS systems
            report.OperatingExpenses.OfficeSupplies = netRevenue * 0.01m; // 1% - packaging, receipts, tech supplies
            report.OperatingExpenses.ProfessionalFees = netRevenue * 0.008m; // 0.8% - accounting, permits, BIR compliance
            
            // Add actual shipping costs to other expenses (warranty, repairs, misc)
            report.OperatingExpenses.OtherExpenses = totalShippingCosts + (netRevenue * 0.012m);

            // Non-operating expenses
            report.OperatingExpenses.InterestExpense = netRevenue * 0.008m; // 0.8% - business loans, credit lines
            report.OperatingExpenses.TaxExpense = totalTaxCosts; // ✅ Use actual tax from orders
        }

        private async Task GenerateMonthlyProfitLossBreakdown(InventoryDbContext context, ProfitAndLossReport report, AnalyticalReportRequest request)
        {
            var monthlyBreakdown = new List<MonthlyProfitLoss>();
            var current = new DateTime(request.StartDate.Year, request.StartDate.Month, 1);

            while (current <= request.EndDate)
            {
                var monthEnd = current.AddMonths(1).AddDays(-1);
                if (monthEnd > request.EndDate) monthEnd = request.EndDate;

                // Get monthly sales (actual data)
                var monthlySales = await context.SalesOrders
                    .Where(so => !so.IsDeleted && 
                               so.OrderDate >= current && 
                               so.OrderDate <= monthEnd &&
                               so.Status != "Cancelled")
                    .SumAsync(so => so.SubTotal);

                // Calculate monthly discounts (actual data)
                var monthlyDiscounts = await context.SalesOrders
                    .Where(so => !so.IsDeleted && 
                               so.OrderDate >= current && 
                               so.OrderDate <= monthEnd &&
                               so.Status != "Cancelled")
                    .SumAsync(so => so.DiscountAmount);

                // Calculate monthly COGS from actual inventory transactions
                var monthlyCOGS = await context.InventoryTransactions
                    .Where(it => !it.IsDeleted && 
                               it.TransactionDate >= current && 
                               it.TransactionDate <= monthEnd &&
                               it.TransactionType == "Sale")
                    .SumAsync(it => it.QuantityChange * it.UnitCost);

                // Calculate monthly operating expenses using same percentage as main report
                var monthlyNetRevenue = monthlySales - monthlyDiscounts;
                var monthlyOpEx = monthlyNetRevenue * 0.308m; // 30.8% total (sum of all tech retail OpEx percentages)

                monthlyBreakdown.Add(new MonthlyProfitLoss
                {
                    Month = current.ToString("MMM yyyy"),
                    Revenue = monthlyNetRevenue, // Use net revenue (after discounts)
                    COGS = monthlyCOGS,
                    OperatingExpenses = monthlyOpEx
                });

                current = current.AddMonths(1);
            }

            report.MonthlyBreakdown = monthlyBreakdown;
        }

        private void GenerateBusinessInsights(ProfitAndLossReport report)
        {
            report.KeyInsights.Clear();
            report.Recommendations.Clear();

            // Key Insights
            report.KeyInsights.Add($"Business is currently {report.ProfitabilityStatus.ToLower()} with a net profit margin of {report.NetProfitMargin:F1}%");
            report.KeyInsights.Add($"Gross profit margin is {report.GrossProfitMargin:F1}%, indicating {(report.GrossProfitMargin >= 40 ? "strong" : report.GrossProfitMargin >= 25 ? "moderate" : "weak")} pricing power");
            report.KeyInsights.Add($"Operating margin is {report.OperatingMargin:F1}%, showing {(report.OperatingMargin >= 15 ? "excellent" : report.OperatingMargin >= 10 ? "good" : "concerning")} operational efficiency");

            // Recommendations based on performance
            if (report.NetIncome < 0)
            {
                report.Recommendations.Add("URGENT: Business is operating at a loss. Review pricing strategy and reduce costs immediately.");
                report.Recommendations.Add("Analyze top loss-making categories and consider discontinuing unprofitable products.");
                report.Recommendations.Add("Implement cost reduction measures in operating expenses, starting with the largest expense categories.");
            }
            else if (report.NetProfitMargin < 5)
            {
                report.Recommendations.Add("Profit margins are thin. Focus on improving operational efficiency and reducing waste.");
                report.Recommendations.Add("Consider raising prices on high-demand products to improve margins.");
                report.Recommendations.Add("Review supplier contracts to negotiate better purchase prices.");
            }
            else if (report.NetProfitMargin >= 15)
            {
                report.Recommendations.Add("Excellent profitability! Consider reinvesting profits into business expansion.");
                report.Recommendations.Add("Explore new product lines or market segments to sustain growth.");
                report.Recommendations.Add("Build cash reserves for future opportunities and economic downturns.");
            }

            // COGS-specific recommendations
            if (report.CostOfGoodsSold.TotalCOGS / report.Revenue.NetRevenue > 0.70m)
            {
                report.Recommendations.Add("COGS is high at {(report.CostOfGoodsSold.TotalCOGS / report.Revenue.NetRevenue * 100):F1}% of revenue. Negotiate better supplier terms.");
            }

            // Operating expense recommendations
            if (report.OperatingExpenses.TotalOperatingExpenses / report.Revenue.NetRevenue > 0.40m)
            {
                report.Recommendations.Add("Operating expenses are high. Review all expense categories for potential savings.");
            }
        }
    }
}
