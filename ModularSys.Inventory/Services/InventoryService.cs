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
    public class InventoryService : IInventoryService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InventoryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // ✅ Shared transactional usage
        public async Task RecordTransactionAsync(InventoryDbContext db, int productId, int quantityChange, decimal amount, string transactionType)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            product.QuantityOnHand += quantityChange;

            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                QuantityChange = quantityChange,
                Amount = amount,
                TransactionType = transactionType,
                TransactionDate = DateTime.UtcNow
            };

            db.InventoryTransactions.Add(transaction);
            // SaveChangesAsync is handled by the caller
        }

        // ✅ Standalone usage — scoped internally
        public async Task<IEnumerable<InventoryTransaction>> GetHistoryAsync(int productId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.InventoryTransactions
                .Where(t => t.ProductId == productId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            return await db.InventoryTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
