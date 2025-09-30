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

            var quantityBefore = product.QuantityOnHand;
            product.QuantityOnHand += quantityChange;
            var quantityAfter = product.QuantityOnHand;

            var transaction = new InventoryTransaction
            {
                TransactionNumber = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                ProductId = productId,
                TransactionDate = DateTime.UtcNow,
                TransactionType = transactionType,
                QuantityBefore = quantityBefore,
                QuantityChange = quantityChange,
                QuantityAfter = quantityAfter,
                UnitCost = quantityChange != 0 ? Math.Abs(amount / quantityChange) : 0,
                Amount = amount,
                Reference = transactionType,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
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
                .Include(t => t.Product)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
