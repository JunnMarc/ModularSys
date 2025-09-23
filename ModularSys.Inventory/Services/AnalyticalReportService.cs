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

            var report = new InventoryTurnoverReport
            {
                ReportDate = request.EndDate,
                AnnualCOGS = salesData.Sum(s => s.TotalCOGS),
                AverageInventoryValue = products.Sum(p => p.QuantityOnHand * p.CostPrice)
            };

            report.OverallTurnoverRatio = report.AverageInventoryValue > 0 ? report.AnnualCOGS / report.AverageInventoryValue : 0;
            report.DaysSalesInInventory = report.OverallTurnoverRatio > 0 ? 365 / report.OverallTurnoverRatio : 0;

            // Product turnover analysis
            report.ProductTurnover = products.Select(p =>
            {
                var sales = salesData.FirstOrDefault(s => s.ProductId == p.ProductId);
                var avgInventory = p.QuantityOnHand * p.CostPrice;
                var cogs = sales?.TotalCOGS ?? 0;
                var turnoverRatio = avgInventory > 0 ? cogs / avgInventory : 0;

                return new ProductTurnover
                {
                    ProductId = p.ProductId.ToString(),
                    ProductName = p.Name,
                    SKU = p.SKU,
                    Category = p.Category?.CategoryName ?? "Uncategorized",
                    TurnoverRatio = turnoverRatio,
                    AverageInventory = avgInventory,
                    COGS = cogs,
                    DaysSalesInInventory = turnoverRatio > 0 ? 365 / turnoverRatio : 0,
                    TurnoverClassification = GetTurnoverClassification(turnoverRatio),
                    RecommendedAction = GetTurnoverRecommendation(turnoverRatio)
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
        private string GetTurnoverClassification(decimal turnoverRatio)
        {
            return turnoverRatio switch
            {
                >= 6 => "Fast",
                >= 3 => "Medium",
                _ => "Slow"
            };
        }

        private string GetTurnoverRecommendation(decimal turnoverRatio)
        {
            return turnoverRatio switch
            {
                >= 6 => "Excellent performance - maintain stock levels",
                >= 3 => "Good performance - monitor regularly",
                >= 1 => "Slow moving - consider promotion or reduction",
                _ => "Very slow - consider discontinuation"
            };
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
                }
            };
        }
    }
}
