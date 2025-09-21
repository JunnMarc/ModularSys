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

            // Get all orders in the date range
            var salesOrders = await db.SalesOrders
                .Include(so => so.Lines)
                .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate)
                .ToListAsync();

            var purchaseOrders = await db.PurchaseOrders
                .Include(po => po.Lines)
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate)
                .ToListAsync();

            // Calculate basic metrics
            var completedSalesOrders = salesOrders.Where(so => so.Status == "Completed").ToList();
            var cancelledSalesOrders = salesOrders.Where(so => so.Status == "Cancelled").ToList();

            analytics.TotalRevenue = completedSalesOrders.Sum(so => so.GrandTotal);
            analytics.TotalCosts = purchaseOrders.Where(po => po.Status == "Received").Sum(po => po.GrandTotal);
            analytics.GrossProfit = analytics.TotalRevenue - analytics.TotalCosts;
            analytics.NetProfit = analytics.GrossProfit; // Simplified - could include operational costs
            analytics.ProfitMargin = analytics.TotalRevenue > 0 ? (analytics.GrossProfit / analytics.TotalRevenue) * 100 : 0;
            analytics.LossAmount = analytics.GrossProfit < 0 ? Math.Abs(analytics.GrossProfit) : 0;

            analytics.TotalOrders = salesOrders.Count;
            analytics.CompletedOrders = completedSalesOrders.Count;
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

        public async Task<List<DailyBusinessMetrics>> GenerateDailyMetricsAsync(InventoryDbContext db, DateTime startDate, DateTime endDate)
        {
            var dailyMetrics = new List<DailyBusinessMetrics>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                var dailySalesOrders = await db.SalesOrders
                    .Include(so => so.Lines)
                    .Where(so => so.OrderDate >= currentDate && so.OrderDate < nextDate)
                    .ToListAsync();

                var dailyPurchaseOrders = await db.PurchaseOrders
                    .Include(po => po.Lines)
                    .Where(po => po.OrderDate >= currentDate && po.OrderDate < nextDate)
                    .ToListAsync();

                var completedOrders = dailySalesOrders.Where(so => so.Status == "Completed").ToList();
                var cancelledOrders = dailySalesOrders.Where(so => so.Status == "Cancelled").ToList();

                var revenue = completedOrders.Sum(so => so.GrandTotal);
                var costs = dailyPurchaseOrders.Where(po => po.Status == "Received").Sum(po => po.GrandTotal);
                var profit = revenue - costs;

                var metrics = new DailyBusinessMetrics
                {
                    Date = currentDate,
                    Revenue = revenue,
                    Costs = costs,
                    Profit = profit > 0 ? profit : 0,
                    Loss = profit < 0 ? Math.Abs(profit) : 0,
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
            var cancelledOrders = await db.SalesOrders
                .Where(so => so.Status == "Cancelled" && 
                           so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
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
            // First, get the raw sales data with simpler queries
            var salesOrderLines = await db.SalesOrderLines
                .Where(sol => sol.SalesOrder.OrderDate >= startDate && 
                             sol.SalesOrder.OrderDate <= endDate &&
                             sol.SalesOrder.Status == "Completed")
                .Select(sol => new
                {
                    sol.ProductId,
                    sol.LineTotal,
                    sol.Quantity,
                    sol.UnitPrice
                })
                .ToListAsync();

            if (!salesOrderLines.Any())
                return new List<ProductProfitability>();

            // Group the data in memory
            var groupedSales = salesOrderLines
                .GroupBy(sol => sol.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Revenue = g.Sum(x => x.LineTotal),
                    UnitsSold = g.Sum(x => x.Quantity),
                    AverageSellingPrice = g.Average(x => x.UnitPrice)
                })
                .ToList();

            // Get product details separately
            var productIds = groupedSales.Select(g => g.ProductId).ToList();
            var products = await db.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            var productProfitability = new List<ProductProfitability>();

            foreach (var sale in groupedSales)
            {
                var product = products.FirstOrDefault(p => p.ProductId == sale.ProductId);
                if (product == null) continue;

                var cost = sale.UnitsSold * (product.CostPrice);
                var profit = sale.Revenue - cost;
                var profitMargin = sale.Revenue > 0 ? (profit / sale.Revenue) * 100 : 0;

                productProfitability.Add(new ProductProfitability
                {
                    ProductId = sale.ProductId,
                    ProductName = product.Name,
                    SKU = product.SKU ?? "",
                    CategoryName = product.Category?.CategoryName ?? "Uncategorized",
                    Revenue = sale.Revenue,
                    Cost = cost,
                    Profit = profit,    
                    ProfitMargin = profitMargin,
                    UnitsSold = sale.UnitsSold,
                    AverageSellingPrice = sale.AverageSellingPrice,
                    AverageCost = product.CostPrice,
                    CurrentStock = product.QuantityOnHand,
                    ProfitabilityRating = GetProfitabilityRating(profitMargin)
                });
            }

            return productProfitability;
        }

        public async Task<List<CustomerAnalytics>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            // Get raw sales order data first
            var salesOrders = await db.SalesOrders
                .Where(so => so.OrderDate >= startDate && 
                           so.OrderDate <= endDate &&
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
