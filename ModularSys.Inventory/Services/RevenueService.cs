using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Finance;
using ModularSys.Inventory.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RevenueService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // ✅ Standalone usage — creates its own DbContext
        public async Task RecordAsync(decimal amount, string source, string reference)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            await RecordAsync(db, amount, source, reference);
            await db.SaveChangesAsync();
        }

        // ✅ Shared transactional usage — uses caller's DbContext
        public async Task RecordAsync(InventoryDbContext db, decimal amount, string source, string reference)
        {
            var entry = new RevenueTransaction
            {
                Amount = amount,
                Source = source,
                Reference = reference,
                Timestamp = DateTime.UtcNow
            };

            db.RevenueTransactions.Add(entry);
            // SaveChangesAsync is handled by the caller
        }

        public async Task<decimal> GetTotalAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            return await db.RevenueTransactions.SumAsync(r => r.Amount);
        }

        public async Task<IEnumerable<RevenueTransaction>> GetAllAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            return await db.RevenueTransactions
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
    }
}
