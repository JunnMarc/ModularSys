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

            // Auto-generate order number if not provided
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }

            order.OrderDate = DateTime.UtcNow;
            order.SubTotal = order.Lines.Sum(l => l.LineTotal);
            order.CreatedAt = DateTime.UtcNow;
            order.CreatedBy = "System";
            
            db.SalesOrders.Add(order);
            await db.SaveChangesAsync();
            return order.SalesOrderId;
        }

        public async Task UpdateAsync(SalesOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

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

            var order = await db.SalesOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.SalesOrderId == salesOrderId);

            if (order == null)
                throw new KeyNotFoundException("Sales order not found.");

            if (order.SubTotal <= 0)
                throw new InvalidOperationException("Sales order has no value.");

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

            await revenue.RecordAsync(
                amount: +order.SubTotal,
                source: "Sale",
                reference: $"SO-{order.SalesOrderId}"
            );

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
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
    }
}
