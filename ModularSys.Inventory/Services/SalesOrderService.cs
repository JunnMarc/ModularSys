using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;

namespace ModularSys.Inventory.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IInventoryService _inventory;
        private readonly IRevenueService _revenue;

        public SalesOrderService(
            IServiceScopeFactory scopeFactory,
            IInventoryService inventory,
            IRevenueService revenue)
        {
            _scopeFactory = scopeFactory;
            _inventory = inventory;
            _revenue = revenue;
        }

        public async Task<int> CreateAsync(SalesOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            // Basic validation for order creation
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order details required");

            if (order.Lines == null || !order.Lines.Any())
                throw new InvalidOperationException("Order must have at least one product");

            // Validate that all products exist (but don't check inventory)
            foreach (var line in order.Lines)
            {
                var productExists = await db.Products.AnyAsync(p => p.ProductId == line.ProductId);
                if (!productExists)
                    throw new KeyNotFoundException($"Product ID {line.ProductId} not found");

                if (line.Quantity <= 0)
                    throw new InvalidOperationException($"Invalid quantity: {line.Quantity}");

                if (line.UnitPrice < 0)
                    throw new InvalidOperationException($"Price cannot be negative: {line.UnitPrice:C}");
            }

            // CRITICAL DEBUG: Check revenue before and after order creation
            var revenueBefore = await db.RevenueTransactions.SumAsync(r => r.Amount);
            Console.WriteLine($"[DEBUG] Revenue BEFORE creating order: ${revenueBefore:F2}");
            
            // NO inventory validation at creation - just create the order as Pending
            
            // Auto-generate order number if not provided
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }

            order.OrderDate = DateTime.UtcNow;
            order.SubTotal = order.Lines.Sum(l => l.LineTotal);
            order.Status = "Pending"; // Always create as Pending
            order.CreatedAt = DateTime.UtcNow;
            order.CreatedBy = "System";
            
            Console.WriteLine($"[DEBUG] About to add order {order.OrderNumber} with status: {order.Status}");
            Console.WriteLine($"[DEBUG] Order total: ${order.SubTotal:F2}");
            Console.WriteLine($"[DEBUG] NO REVENUE SHOULD BE RECORDED - this is just creating a pending order");
            
            db.SalesOrders.Add(order);
            await db.SaveChangesAsync();
            
            var revenueAfter = await db.RevenueTransactions.SumAsync(r => r.Amount);
            Console.WriteLine($"[DEBUG] Revenue AFTER creating order: ${revenueAfter:F2}");
            
            if (revenueAfter != revenueBefore)
            {
                Console.WriteLine($"[DEBUG] ⚠️ PROBLEM FOUND! Revenue changed by ${revenueAfter - revenueBefore:F2}");
                Console.WriteLine($"[DEBUG] This indicates there's a trigger, interceptor, or other process adding revenue!");
                
                // Let's see what revenue transactions were added
                var newTransactions = await db.RevenueTransactions
                    .Where(r => r.Timestamp > DateTime.UtcNow.AddMinutes(-1))
                    .ToListAsync();
                    
                foreach (var trans in newTransactions)
                {
                    Console.WriteLine($"[DEBUG] New revenue transaction: {trans.Source} - {trans.Reference} - ${trans.Amount:F2} at {trans.Timestamp}");
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] ✅ CORRECT: No revenue was recorded for pending order");
            }
            
            return order.SalesOrderId;
        }

        public async Task UpdateAsync(SalesOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            // Basic validation
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order details required");

            var existingOrder = await db.SalesOrders.FirstOrDefaultAsync(o => o.SalesOrderId == order.SalesOrderId);
            if (existingOrder == null)
                throw new KeyNotFoundException($"Order {order.SalesOrderId} not found");

            if (existingOrder.Status == "Completed")
                throw new InvalidOperationException($"Cannot modify completed order {existingOrder.OrderNumber}");

            if (existingOrder.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot modify cancelled order {existingOrder.OrderNumber}");

            // NO inventory validation during updates - allow modifications to pending orders
            
            // Recalculate totals
            order.SubTotal = order.Lines.Sum(l => l.LineTotal);
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = "System";

            db.SalesOrders.Update(order);
            await db.SaveChangesAsync();
        }

        public async Task CompleteAsync(int salesOrderId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();
            var revenue = scope.ServiceProvider.GetRequiredService<IRevenueService>();

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var order = await db.SalesOrders
                    .Include(o => o.Lines)
                    .ThenInclude(l => l.Product)
                    .FirstOrDefaultAsync(o => o.SalesOrderId == salesOrderId);

                if (order == null)
                    throw new KeyNotFoundException($"Order {salesOrderId} not found");

                if (order.Status == "Completed")
                    throw new InvalidOperationException($"Order {order.OrderNumber} already completed");

                if (order.Status == "Cancelled")
                    throw new InvalidOperationException($"Cannot complete cancelled order {order.OrderNumber}");

                if (order.SubTotal <= 0)
                    throw new InvalidOperationException($"Order {order.OrderNumber} has no value");

                if (!order.Lines.Any())
                    throw new InvalidOperationException($"Order {order.OrderNumber} is empty");

                await ValidateInventoryAvailabilityWithDetailedErrorsAsync(db, order);

                // Process inventory transactions
                foreach (var line in order.Lines)
                {
                    await inventory.RecordTransactionAsync(
                        db,
                        productId: line.ProductId,
                        quantityChange: -line.Quantity,
                        amount: line.LineTotal,
                        transactionType: "Sale"
                    );
                }

                // Record revenue ONLY when completing the order
                await revenue.RecordAsync(
                    amount: +order.SubTotal,
                    source: "Sale",
                    reference: $"SO-{order.SalesOrderId}"
                );

                // Update order status to completed
                order.Status = "Completed";
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = "System";

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Success - no console log needed (UI will show toast)
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[SYSTEM STATUS] Order completion failed: {ex.Message}");
                throw; // Re-throw the original exception
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[SYSTEM STATUS] Critical error during order completion: {ex.Message}");
                throw new InvalidOperationException($"System error during order completion: {ex.Message}", ex);
            }
        }

        public async Task CancelAsync(int salesOrderId, string cancellationReason, string cancelledBy)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var order = await db.SalesOrders
                .FirstOrDefaultAsync(o => o.SalesOrderId == salesOrderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {salesOrderId} not found");

            if (order.Status == "Completed")
                throw new InvalidOperationException($"Cannot cancel completed order {order.OrderNumber}");

            if (order.Status == "Cancelled")
                throw new InvalidOperationException($"Order {order.OrderNumber} already cancelled");

            // Update order status and cancellation details
            order.Status = "Cancelled";
            order.CancellationReason = cancellationReason;
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledBy = cancelledBy;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = cancelledBy;

            await db.SaveChangesAsync();
        }

        public async Task<SalesOrder?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var query = db.SalesOrders.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();

            return await query
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.SalesOrderId == id);
        }

        public async Task<IEnumerable<SalesOrder>> GetAllAsync(bool includeDeleted = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var query = db.SalesOrders.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();

            return await query
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.SalesOrders
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<(bool CanComplete, List<string> Issues)> CheckInventoryAvailabilityAsync(int salesOrderId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var order = await db.SalesOrders
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.SalesOrderId == salesOrderId);

            if (order == null)
                return (false, new List<string> { $"Order {salesOrderId} not found" });

            if (order.Status != "Pending")
                return (false, new List<string> { $"Order {order.OrderNumber} not pending (Status: {order.Status})" });

            var issues = new List<string>();

            foreach (var line in order.Lines)
            {
                var product = line.Product ?? await db.Products.FirstOrDefaultAsync(p => p.ProductId == line.ProductId);
                if (product == null)
                {
                    issues.Add($"Product ID {line.ProductId} not found");
                    continue;
                }

                if (product.QuantityOnHand < line.Quantity)
                {
                    var shortage = line.Quantity - product.QuantityOnHand;
                    issues.Add($"{product.Name}: Need {line.Quantity}, Have {product.QuantityOnHand}, Short {shortage}");
                }
            }

            return (issues.Count == 0, issues);
        }

        public async Task<(decimal TotalRevenueBefore, decimal TotalRevenueAfter)> CreateAsyncWithRevenueCheck(SalesOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var revenueService = scope.ServiceProvider.GetRequiredService<IRevenueService>();

            // Check revenue before creating order
            var revenueBefore = await revenueService.GetTotalAsync();
            Console.WriteLine($"[REVENUE CHECK] Total revenue BEFORE creating order: ${revenueBefore:F2}");

            // Create the order (should NOT add revenue)
            var orderId = await CreateAsync(order);
            Console.WriteLine($"[REVENUE CHECK] Order {orderId} created with status: Pending");

            // Check revenue after creating order
            var revenueAfter = await revenueService.GetTotalAsync();
            Console.WriteLine($"[REVENUE CHECK] Total revenue AFTER creating order: ${revenueAfter:F2}");

            if (revenueAfter != revenueBefore)
            {
                Console.WriteLine($"[REVENUE CHECK] ⚠️ WARNING: Revenue changed by ${revenueAfter - revenueBefore:F2} when creating pending order!");
                Console.WriteLine($"[REVENUE CHECK] This should NOT happen - revenue should only change when completing orders");
            }
            else
            {
                Console.WriteLine($"[REVENUE CHECK] ✅ CORRECT: Revenue unchanged when creating pending order");
            }

            return (revenueBefore, revenueAfter);
        }

        public async Task DeleteAsync(int id, string deletedBy)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var order = await db.SalesOrders.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.SalesOrderId == id);
            if (order != null)
            {
                order.IsDeleted = true;
                order.DeletedAt = DateTime.UtcNow;
                order.DeletedBy = deletedBy;
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var order = await db.SalesOrders.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.SalesOrderId == id && o.IsDeleted);
            if (order == null) return false;
            order.IsDeleted = false;
            order.DeletedAt = null;
            order.DeletedBy = null;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = restoredBy;
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates inventory availability with detailed error reporting for order completion
        /// </summary>
        private async Task ValidateInventoryAvailabilityWithDetailedErrorsAsync(InventoryDbContext db, SalesOrder order)
        {
            var inventoryErrors = new List<string>();
            var systemStatus = new List<string>();

            Console.WriteLine($"[SYSTEM STATUS] Validating inventory for order {order.OrderNumber}...");

            foreach (var line in order.Lines)
            {
                var product = line.Product ?? await db.Products.FirstOrDefaultAsync(p => p.ProductId == line.ProductId);
                if (product == null)
                {
                    var error = $"Product ID {line.ProductId} not found";
                    inventoryErrors.Add(error);
                    systemStatus.Add($"[ERROR] {error}");
                    continue;
                }

                systemStatus.Add($"[CHECK] Product: {product.Name} | Available: {product.QuantityOnHand} | Requested: {line.Quantity}");

                if (product.QuantityOnHand < line.Quantity)
                {
                    var shortage = line.Quantity - product.QuantityOnHand;
                    var error = $"{product.Name}: Need {line.Quantity}, Have {product.QuantityOnHand}, Short {shortage}";
                    inventoryErrors.Add(error);
                    systemStatus.Add($"[SHORTAGE] {error}");
                }
                else
                {
                    systemStatus.Add($"[OK] {product.Name} - Stock OK ({product.QuantityOnHand} units)");
                }
            }

            // Log all system status messages
            foreach (var status in systemStatus)
            {
                Console.WriteLine(status);
            }

            // If there are inventory errors, throw detailed exception
            if (inventoryErrors.Any())
            {
                var errorMessage = $"Insufficient inventory for order {order.OrderNumber}:\n" +
                                 string.Join("\n", inventoryErrors.Select((error, index) => $"{index + 1}. {error}"));
                
                Console.WriteLine($"[SYSTEM STATUS] Inventory validation FAILED for order {order.OrderNumber} - {inventoryErrors.Count} issues");
                
                throw new InvalidOperationException(errorMessage);
            }

            Console.WriteLine($"[SYSTEM STATUS] Inventory validation PASSED for order {order.OrderNumber}");
        }
    }
}
