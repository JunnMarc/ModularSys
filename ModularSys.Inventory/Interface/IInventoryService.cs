using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Interface
{
    public interface IInventoryService
    {
        Task RecordTransactionAsync(InventoryDbContext db, int productId, int quantityChange, decimal amount, string transactionType);
        Task<IEnumerable<InventoryTransaction>> GetHistoryAsync(int productId);
        Task<IEnumerable<InventoryTransaction>> GetAllAsync();
    }
}
