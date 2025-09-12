using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            order.TotalAmount = order.Lines.Sum(l => l.LineTotal);
            db.SalesOrders.Add(order);
            await db.SaveChangesAsync();
            return order.SalesOrderId;
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

            if (order.TotalAmount <= 0)
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
                amount: +order.GrandTotal,
                source: "Sale",
                reference: $"SO-{order.SalesOrderId}"
            );

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }


        public async Task<SalesOrder?> GetByIdAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.SalesOrders
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.SalesOrderId == id);
        }

        public async Task<IEnumerable<SalesOrder>> GetAllAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.SalesOrders
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}
