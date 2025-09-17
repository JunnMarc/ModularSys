using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;

namespace ModularSys.Inventory.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRevenueService _revenue;
        private readonly IInventoryService _inventory;

        public PurchaseOrderService(
            IServiceScopeFactory scopeFactory,
            IRevenueService revenue,
            IInventoryService inventory)
        {
            _scopeFactory = scopeFactory;
            _revenue = revenue;
            _inventory = inventory;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.PurchaseOrders
                .Include(p => p.Lines)
                .ThenInclude(l => l.Product)
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.PurchaseOrders
                .Include(p => p.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);
        }

        public async Task<int> CreateAsync(PurchaseOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            order.TotalAmount = order.Lines.Sum(l => l.LineTotal);
            db.PurchaseOrders.Add(order);
            await db.SaveChangesAsync();
            return order.PurchaseOrderId;
        }

        public async Task UpdateAsync(PurchaseOrder order)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var existing = await db.PurchaseOrders
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == order.PurchaseOrderId);

            if (existing == null)
                throw new KeyNotFoundException("Purchase order not found.");

            db.Entry(existing).CurrentValues.SetValues(order);
            existing.Lines = order.Lines;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var order = await db.PurchaseOrders.FindAsync(id);
            if (order != null)
            {
                db.PurchaseOrders.Remove(order);
                await db.SaveChangesAsync();
            }
        }

        public async Task ReceiveAsync(int purchaseOrderId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var revenue = scope.ServiceProvider.GetRequiredService<IRevenueService>();
            var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            using var transaction = await db.Database.BeginTransactionAsync();

            var order = await db.PurchaseOrders
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

            if (order == null)
                throw new KeyNotFoundException("Purchase order not found.");

            if (order.TotalAmount <= 0)
                throw new InvalidOperationException("Purchase order has no value.");

            order.Status = "Received";

            // Record inventory transactions
            foreach (var line in order.Lines)
            {
                await inventory.RecordTransactionAsync(
                    db,
                    productId: line.ProductId,
                    quantityChange: line.Quantity,
                    amount: line.LineTotal,
                    transactionType: "Purchase"
                );
            }

            // Record negative revenue
            await revenue.RecordAsync(
                db,
                amount: -order.TotalAmount,
                source: "Purchase",
                reference: $"PO-{order.PurchaseOrderId}"
            );

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
