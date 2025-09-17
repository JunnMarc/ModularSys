using ModularSys.Data.Common.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Interface
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync(bool includeDeleted = false);
        Task<PurchaseOrder?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<int> CreateAsync(PurchaseOrder order);
        Task UpdateAsync(PurchaseOrder order);
        Task DeleteAsync(int id, string deletedBy);
        Task<bool> RestoreAsync(int id, string restoredBy);
        Task ReceiveAsync(int purchaseOrderId);
    }
}
