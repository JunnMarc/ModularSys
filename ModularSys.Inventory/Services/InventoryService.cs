using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Inventory.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ModularSysDbContext _db;

        public InventoryService(ModularSysDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllItemsAsync()
        {
            return await _db.InventoryItems.AsNoTracking().ToListAsync();
        }

        public async Task<InventoryItem?> GetItemByIdAsync(int id)
        {
            return await _db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<InventoryItem> AddItemAsync(InventoryItem item)
        {
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
        {
            var existing = await _db.InventoryItems.FindAsync(item.Id);
            if (existing == null) throw new KeyNotFoundException($"Item with ID {item.Id} not found");

            _db.Entry(existing).CurrentValues.SetValues(item);
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null) return false;

            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}