using ModularSys.Data.Common.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Interface
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<int> CreateAsync(PurchaseOrder order);
        Task UpdateAsync(PurchaseOrder order);
        Task DeleteAsync(int id);
        Task ReceiveAsync(int purchaseOrderId);
    }
}
