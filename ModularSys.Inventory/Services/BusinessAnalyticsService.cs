using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Services
{
    public class BusinessAnalyticsService : IBusinessAnalyticsService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BusinessAnalyticsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<BusinessAnalyticsData> GetBusinessAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var analytics = new BusinessAnalyticsData
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // FIXED: Get sales order lines with proper COGS calculation
            var completedSalesOrderLines = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate && 
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted)
                .ToListAsync();

            var cancelledSalesOrders = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
                           so.Status == "Cancelled" &&
                           !so.IsDeleted)
                .ToListAsync();

            var allSalesOrders = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
                           !so.IsDeleted)
                .ToListAsync();

            // FIXED: Calculate proper revenue and COGS
            analytics.TotalRevenue = completedSalesOrderLines.Sum(sol => sol.LineTotal);
            analytics.TotalCosts = await CalculateActualCOGSAsync(db, completedSalesOrderLines);
            analytics.GrossProfit = analytics.TotalRevenue - analytics.TotalCosts;
            
            // Calculate actual operational costs from real data
            var actualShippingCosts = await db.SalesOrders
                .Where(so => !so.IsDeleted && so.OrderDate >= startDate && so.OrderDate <= endDate && so.Status != "Cancelled")
                .SumAsync(so => so.ShippingCost);
            
            // Calculate tax costs using the formula since TaxAmount is NotMapped
            var actualTaxCosts = await db.SalesOrders
                .Where(so => !so.IsDeleted && so.OrderDate >= startDate && so.OrderDate <= endDate && so.Status != "Cancelled")
                .SumAsync(so => (so.SubTotal - so.DiscountAmount) * so.TaxRate);
            
            // Real operational costs = shipping + taxes (actual business expenses)
            var operationalCosts = actualShippingCosts + actualTaxCosts;
            analytics.NetProfit = analytics.GrossProfit - operationalCosts;
            
            analytics.ProfitMargin = analytics.TotalRevenue > 0 ? (analytics.GrossProfit / analytics.TotalRevenue) * 100 : 0;
            analytics.LossAmount = analytics.GrossProfit < 0 ? Math.Abs(analytics.GrossProfit) : 0;

            // Order statistics
            analytics.TotalOrders = allSalesOrders.Count;
            analytics.CompletedOrders = allSalesOrders.Count(so => so.Status == "Completed");
            analytics.CancelledOrders = cancelledSalesOrders.Count;
            analytics.CancellationRate = analytics.TotalOrders > 0 ? (analytics.CancelledOrders / (decimal)analytics.TotalOrders) * 100 : 0;
            analytics.AverageOrderValue = analytics.CompletedOrders > 0 ? analytics.TotalRevenue / analytics.CompletedOrders : 0;

            // Generate daily metrics
            analytics.DailyMetrics = await GenerateDailyMetricsAsync(db, startDate, endDate);

            // Analyze cancellation reasons
            analytics.CancellationReasons = await AnalyzeCancellationReasonsAsync(db, startDate, endDate);

            // Product profitability analysis
            analytics.TopProfitableProducts = await GetTopProfitableProductsAsync(db, startDate, endDate, 10);
            analytics.LeastProfitableProducts = await GetLeastProfitableProductsAsync(db, startDate, endDate, 10);

            return analytics;
        }

        // FIXED: Calculate actual COGS based on product cost prices at time of sale
        private async Task<decimal> CalculateActualCOGSAsync(InventoryDbContext db, List<SalesOrderLine> salesOrderLines)
        {
            decimal totalCOGS = 0;

            foreach (var line in salesOrderLines)
            {
                // Use the product's cost price (in a real system, this would be the weighted average cost)
                // For now, using current cost price as approximation
                var costPerUnit = line.Product?.CostPrice ?? 0;
                totalCOGS += line.Quantity * costPerUnit;
            }

            return totalCOGS;
        }

        public async Task<List<DailyBusinessMetrics>> GenerateDailyMetricsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var dailyMetrics = new List<DailyBusinessMetrics>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                // FIXED: Get sales order lines for proper COGS calculation
                var dailySalesOrderLines = await db.SalesOrderLines
                    .Include(sol => sol.Product)
                    .Include(sol => sol.SalesOrder)
                    .Where(sol => sol.SalesOrder.OrderDate >= currentDate && 
                                 sol.SalesOrder.OrderDate < nextDate &&
                                 sol.SalesOrder.Status == "Completed" &&
                                 !sol.SalesOrder.IsDeleted)
                    .ToListAsync();

                var dailySalesOrders = await db.SalesOrders
                    .Where(so => so.OrderDate >= currentDate && 
                               so.OrderDate < nextDate &&
                               !so.IsDeleted)
                    .ToListAsync();

                var completedOrders = dailySalesOrders.Where(so => so.Status == "Completed").ToList();
                var cancelledOrders = dailySalesOrders.Where(so => so.Status == "Cancelled").ToList();

                // FIXED: Calculate proper revenue and COGS for the day
                var revenue = dailySalesOrderLines.Sum(sol => sol.LineTotal);
                var costs = await CalculateActualCOGSAsync(db, dailySalesOrderLines);
                var grossProfit = revenue - costs;
                
                // Calculate actual daily operational costs
                var dailyShippingCosts = await db.SalesOrders
                    .Where(so => !so.IsDeleted && so.OrderDate.Date == currentDate.Date && so.Status != "Cancelled")
                    .SumAsync(so => so.ShippingCost);
                
                // Calculate daily tax costs using the formula since TaxAmount is NotMapped
                var dailyTaxCosts = await db.SalesOrders
                    .Where(so => !so.IsDeleted && so.OrderDate.Date == currentDate.Date && so.Status != "Cancelled")
                    .SumAsync(so => (so.SubTotal - so.DiscountAmount) * so.TaxRate);
                
                var operationalCosts = dailyShippingCosts + dailyTaxCosts;
                var netProfit = grossProfit - operationalCosts;

                var metrics = new DailyBusinessMetrics
                {
                    Date = currentDate,
                    Revenue = revenue,
                    Costs = costs,
                    Profit = netProfit > 0 ? netProfit : 0,
                    Loss = netProfit < 0 ? Math.Abs(netProfit) : 0,
                    OrdersCompleted = completedOrders.Count,
                    OrdersCancelled = cancelledOrders.Count,
                    CancellationRate = dailySalesOrders.Count > 0 ? (cancelledOrders.Count / (decimal)dailySalesOrders.Count) * 100 : 0,
                    AverageOrderValue = completedOrders.Count > 0 ? revenue / completedOrders.Count : 0
                };

                dailyMetrics.Add(metrics);
                currentDate = currentDate.AddDays(1);
            }

            return dailyMetrics;
        }

        public async Task<List<CancellationAnalysis>> AnalyzeCancellationReasonsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            // FIXED: Added soft delete filtering
            var cancelledOrders = await db.SalesOrders
                .Where(so => so.Status == "Cancelled" && 
                           so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
                           !so.IsDeleted &&
                           !string.IsNullOrEmpty(so.CancellationReason))
                .ToListAsync();

            var totalCancelled = cancelledOrders.Count;
            if (totalCancelled == 0) return new List<CancellationAnalysis>();

            var reasonGroups = cancelledOrders
                .GroupBy(so => so.CancellationReason ?? "Unknown")
                .Select(g => new CancellationAnalysis
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Percentage = (g.Count() / (decimal)totalCancelled) * 100,
                    LostRevenue = g.Sum(so => so.GrandTotal),
                    FirstOccurrence = g.Min(so => so.CancelledAt),
                    LastOccurrence = g.Max(so => so.CancelledAt),
                    Impact = GetImpactLevel(g.Count(), totalCancelled)
                })
                .OrderByDescending(ca => ca.Count)
                .ToList();

            return reasonGroups;
        }

        public async Task<List<ProductProfitability>> GetTopProfitableProductsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate, int count)
        {
            var productProfitability = await GetProductProfitabilityAsync(db, startDate, endDate);
            return productProfitability
                .OrderByDescending(pp => pp.ProfitMargin)
                .Take(count)
                .ToList();
        }

        public async Task<List<ProductProfitability>> GetLeastProfitableProductsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate, int count)
        {
            var productProfitability = await GetProductProfitabilityAsync(db, startDate, endDate);
            return productProfitability
                .OrderBy(pp => pp.ProfitMargin)
                .Take(count)
                .ToList();
        }

        private async Task<List<ProductProfitability>> GetProductProfitabilityAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            // FIXED: Get sales data with product information for accurate cost calculation
            var salesOrderLines = await db.SalesOrderLines
                .Include(sol => sol.Product)
                .ThenInclude(p => p.Category)
                .Include(sol => sol.SalesOrder)
                .Where(sol => sol.SalesOrder.OrderDate >= startDate && 
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed" &&
                             !sol.SalesOrder.IsDeleted &&
                             sol.Product != null &&
                             !sol.Product.IsDeleted)
                .ToListAsync();

            if (!salesOrderLines.Any())
                return new List<ProductProfitability>();

            // Group by product and calculate profitability
            var productProfitability = salesOrderLines
                .GroupBy(sol => sol.ProductId)
                .Select(g => 
                {
                    var firstLine = g.First();
                    var product = firstLine.Product;
                    
                    var revenue = g.Sum(sol => sol.LineTotal);
                    var unitsSold = g.Sum(sol => sol.Quantity);
                    var averageSellingPrice = g.Average(sol => sol.UnitPrice);
                    
                    // FIXED: Use product's cost price for accurate COGS calculation
                    var cost = unitsSold * (product?.CostPrice ?? 0);
                    var profit = revenue - cost;
                    var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;

                    return new ProductProfitability
                    {
                        ProductId = g.Key,
                        ProductName = product?.Name ?? "Unknown Product",
                        SKU = product?.SKU ?? "",
                        CategoryName = product?.Category?.CategoryName ?? "Uncategorized",
                        Revenue = revenue,
                        Cost = cost,
                        Profit = profit,    
                        ProfitMargin = profitMargin,
                        UnitsSold = unitsSold,
                        AverageSellingPrice = averageSellingPrice,
                        AverageCost = product?.CostPrice ?? 0,
                        CurrentStock = product?.QuantityOnHand ?? 0,
                        ProfitabilityRating = GetProfitabilityRating(profitMargin)
                    };
                })
                .ToList();

            return productProfitability;
        }

        public async Task<List<CustomerAnalytics>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            // FIXED: Get raw sales order data with soft delete filtering
            var salesOrders = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
                           !so.IsDeleted &&
                           !string.IsNullOrEmpty(so.CustomerName))
                .Select(so => new
                {
                    so.CustomerName,
                    so.CustomerEmail,
                    so.Status,
                    so.GrandTotal,
                    so.OrderDate
                })
                .ToListAsync();

            if (!salesOrders.Any())
                return new List<CustomerAnalytics>();

            // Group and calculate in memory
            var customerData = salesOrders
                .GroupBy(so => new { so.CustomerName, so.CustomerEmail })
                .Select(g => new CustomerAnalytics
                {
                    CustomerName = g.Key.CustomerName ?? "Unknown",
                    CustomerEmail = g.Key.CustomerEmail ?? "",
                    TotalOrders = g.Count(),
                    CompletedOrders = g.Count(so => so.Status == "Completed"),
                    CancelledOrders = g.Count(so => so.Status == "Cancelled"),
                    TotalRevenue = g.Where(so => so.Status == "Completed").Sum(so => so.GrandTotal),
                    FirstOrderDate = g.Min(so => so.OrderDate),
                    LastOrderDate = g.Max(so => so.OrderDate)
                })
                .ToList();

            foreach (var customer in customerData)
            {
                customer.AverageOrderValue = customer.CompletedOrders > 0 ? customer.TotalRevenue / customer.CompletedOrders : 0;
                customer.CancellationRate = customer.TotalOrders > 0 ? (customer.CancelledOrders / (decimal)customer.TotalOrders) * 100 : 0;
                customer.CustomerSegment = GetCustomerSegment(customer);
            }

            return customerData.OrderByDescending(ca => ca.TotalRevenue).ToList();
        }

        private string GetImpactLevel(int count, int total)
        {
            var percentage = (count / (decimal)total) * 100;
            return percentage switch
            {
                >= 30 => "High",
                >= 15 => "Medium",
                _ => "Low"
            };
        }

        private string GetProfitabilityRating(decimal profitMargin)
        {
            return profitMargin switch
            {
                >= 30 => "Excellent",
                >= 20 => "Good",
                >= 10 => "Fair",
                _ => "Poor"
            };
        }

        private string GetCustomerSegment(CustomerAnalytics customer)
        {
            if (customer.TotalRevenue >= 10000 && customer.CancellationRate < 10)
                return "VIP";
            if (customer.TotalRevenue >= 5000 && customer.CancellationRate < 20)
                return "Regular";
            if (customer.TotalOrders <= 2)
                return "New";
            if (customer.CancellationRate >= 50)
                return "At-Risk";
            return "Regular";
        }
    }
}
