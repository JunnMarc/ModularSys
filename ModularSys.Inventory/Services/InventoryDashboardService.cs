using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;

namespace ModularSys.Inventory.Services
{
    public class InventoryDashboardService : IInventoryDashboardService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InventoryDashboardService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<InventoryDashboardData> GetDashboardDataAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // Get basic counts
            var totalProducts = await db.Products.CountAsync();
            var totalCategories = await db.Categories.CountAsync();
            
            // Get products with stock info
            var products = await db.Products.Include(p => p.Category).ToListAsync();
            var lowStockProducts = products.Where(p => p.QuantityOnHand <= p.ReorderLevel).ToList();
            var outOfStockProducts = products.Where(p => p.QuantityOnHand == 0).ToList();
            var expiringProducts = products.Where(p => p.ExpiryDate.HasValue && 
                p.ExpiryDate.Value <= now.AddDays(30)).ToList();

            // Calculate inventory value
            var totalInventoryValue = products.Sum(p => p.QuantityOnHand * p.CostPrice);

            // Get order counts
            var pendingSalesOrders = await db.SalesOrders
                .CountAsync(so => so.Status == "Pending");
            var pendingPurchaseOrders = await db.PurchaseOrders
                .CountAsync(po => po.Status == "Pending");

            // Get revenue data
            var monthlyRevenue = await db.RevenueTransactions
                .Where(rt => rt.Timestamp >= monthStart && rt.Amount > 0)
                .SumAsync(rt => rt.Amount);

            var monthlyCosts = await db.RevenueTransactions
                .Where(rt => rt.Timestamp >= monthStart && rt.Amount < 0)
                .SumAsync(rt => Math.Abs(rt.Amount));

            var profitMargin = monthlyRevenue > 0 ? ((monthlyRevenue - monthlyCosts) / monthlyRevenue) * 100 : 0;

            var totalTransactions = await db.InventoryTransactions
                .CountAsync(it => it.TransactionDate >= monthStart);

            return new InventoryDashboardData
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalInventoryValue = totalInventoryValue,
                LowStockCount = lowStockProducts.Count,
                OutOfStockCount = outOfStockProducts.Count,
                ExpiringProductsCount = expiringProducts.Count,
                PendingSalesOrders = pendingSalesOrders,
                PendingPurchaseOrders = pendingPurchaseOrders,
                MonthlyRevenue = monthlyRevenue,
                MonthlyCosts = monthlyCosts,
                ProfitMargin = profitMargin,
                TotalTransactions = totalTransactions
            };
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.Products
                .Include(p => p.Category)
                .Where(p => p.QuantityOnHand <= p.ReorderLevel)
                .OrderBy(p => p.QuantityOnHand)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysAhead = 30)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

            return await db.Products
                .Include(p => p.Category)
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= cutoffDate)
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryStockSummary>> GetCategoryStockSummaryAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var categories = await db.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return categories.Select(c => new CategoryStockSummary
            {
                CategoryName = c.CategoryName,
                Icon = c.Icon,
                Color = c.Color,
                ProductCount = c.Products.Count,
                TotalStock = c.Products.Sum(p => p.QuantityOnHand),
                TotalValue = c.Products.Sum(p => p.QuantityOnHand * p.CostPrice),
                LowStockCount = c.Products.Count(p => p.QuantityOnHand <= p.ReorderLevel),
                IsRevenueCritical = c.IsRevenueCritical
            }).OrderByDescending(c => c.TotalValue);
        }

        public async Task<IEnumerable<InventoryMovementData>> GetInventoryMovementDataAsync(int days = 30)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var startDate = DateTime.UtcNow.AddDays(-days);

            var transactions = await db.InventoryTransactions
                .Where(it => it.TransactionDate >= startDate)
                .GroupBy(it => it.TransactionDate.Date)
                .Select(g => new InventoryMovementData
                {
                    Date = g.Key,
                    StockIn = g.Where(t => t.QuantityChange > 0).Sum(t => t.QuantityChange),
                    StockOut = Math.Abs(g.Where(t => t.QuantityChange < 0).Sum(t => t.QuantityChange)),
                    ValueIn = g.Where(t => t.QuantityChange > 0).Sum(t => t.Amount),
                    ValueOut = Math.Abs(g.Where(t => t.QuantityChange < 0).Sum(t => t.Amount)),
                    MovementType = "Daily"
                })
                .OrderBy(m => m.Date)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<TopSellingProduct>> GetTopSellingProductsAsync(int days = 30, int count = 10)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var startDate = DateTime.UtcNow.AddDays(-days);

            var topProducts = await db.InventoryTransactions
                .Where(it => it.TransactionDate >= startDate && it.TransactionType == "Sale")
                .Include(it => it.Product)
                .GroupBy(it => new { it.ProductId, it.Product.Name, it.Product.SKU, it.Product.UnitPrice, it.Product.QuantityOnHand })
                .Select(g => new TopSellingProduct
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    SKU = g.Key.SKU,
                    QuantitySold = Math.Abs(g.Sum(t => t.QuantityChange)),
                    Revenue = Math.Abs(g.Sum(t => t.Amount)),
                    UnitPrice = g.Key.UnitPrice,
                    CurrentStock = g.Key.QuantityOnHand
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        public async Task<RevenueAnalytics> GetRevenueAnalyticsAsync(int days = 30)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var startDate = DateTime.UtcNow.AddDays(-days);

            var revenueTransactions = await db.RevenueTransactions
                .Where(rt => rt.Timestamp >= startDate)
                .ToListAsync();

            var totalRevenue = revenueTransactions.Where(rt => rt.Amount > 0).Sum(rt => rt.Amount);
            var totalCosts = Math.Abs(revenueTransactions.Where(rt => rt.Amount < 0).Sum(rt => rt.Amount));
            var grossProfit = totalRevenue - totalCosts;
            var profitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            var dailyRevenues = revenueTransactions
                .GroupBy(rt => rt.Timestamp.Date)
                .Select(g => new DailyRevenue
                {
                    Date = g.Key,
                    Revenue = g.Where(rt => rt.Amount > 0).Sum(rt => rt.Amount),
                    Costs = Math.Abs(g.Where(rt => rt.Amount < 0).Sum(rt => rt.Amount)),
                    Profit = g.Where(rt => rt.Amount > 0).Sum(rt => rt.Amount) - 
                            Math.Abs(g.Where(rt => rt.Amount < 0).Sum(rt => rt.Amount))
                })
                .OrderBy(dr => dr.Date);

            return new RevenueAnalytics
            {
                TotalRevenue = totalRevenue,
                TotalCosts = totalCosts,
                GrossProfit = grossProfit,
                ProfitMargin = profitMargin,
                DailyRevenues = dailyRevenues
            };
        }

        public async Task<IEnumerable<StockAlert>> GetStockAlertsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alerts = new List<StockAlert>();
            var now = DateTime.UtcNow;

            var products = await db.Products
                .Include(p => p.Category)
                .ToListAsync();

            foreach (var product in products)
            {
                // Out of stock alerts
                if (product.QuantityOnHand == 0)
                {
                    alerts.Add(new StockAlert
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        SKU = product.SKU,
                        AlertType = "OutOfStock",
                        Message = $"{product.Name} is out of stock",
                        CurrentStock = product.QuantityOnHand,
                        ReorderLevel = product.ReorderLevel,
                        MinStockLevel = product.MinStockLevel,
                        Severity = "Critical",
                        CreatedAt = now
                    });
                }
                // Low stock alerts
                else if (product.QuantityOnHand <= product.ReorderLevel)
                {
                    alerts.Add(new StockAlert
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        SKU = product.SKU,
                        AlertType = "LowStock",
                        Message = $"{product.Name} is running low (Stock: {product.QuantityOnHand}, Reorder Level: {product.ReorderLevel})",
                        CurrentStock = product.QuantityOnHand,
                        ReorderLevel = product.ReorderLevel,
                        MinStockLevel = product.MinStockLevel,
                        Severity = product.QuantityOnHand <= (product.MinStockLevel ?? 0) ? "Error" : "Warning",
                        CreatedAt = now
                    });
                }

                // Expiring product alerts
                if (product.ExpiryDate.HasValue)
                {
                    var daysUntilExpiry = (product.ExpiryDate.Value - now).Days;
                    if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0)
                    {
                        var severity = daysUntilExpiry <= 7 ? "Critical" : daysUntilExpiry <= 14 ? "Error" : "Warning";
                        alerts.Add(new StockAlert
                        {
                            ProductId = product.ProductId,
                            ProductName = product.Name,
                            SKU = product.SKU,
                            AlertType = "Expiring",
                            Message = $"{product.Name} expires in {daysUntilExpiry} days",
                            CurrentStock = product.QuantityOnHand,
                            ExpiryDate = product.ExpiryDate,
                            Severity = severity,
                            CreatedAt = now
                        });
                    }
                }
            }

            return alerts.OrderByDescending(a => a.Severity).ThenBy(a => a.AlertType);
        }
    }
}
