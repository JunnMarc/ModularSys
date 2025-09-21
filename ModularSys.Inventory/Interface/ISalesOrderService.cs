using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Inventory.Interface
{
    public interface ISalesOrderService
    {
        Task<int> CreateAsync(SalesOrder order);
        Task UpdateAsync(SalesOrder order);
        Task CompleteAsync(int salesOrderId);
        Task CancelAsync(int salesOrderId, string cancellationReason, string cancelledBy);
        Task<SalesOrder?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<SalesOrder>> GetAllAsync(bool includeDeleted = false);
        Task DeleteAsync(int id, string deletedBy);
        Task<bool> RestoreAsync(int id, string restoredBy);
    }
}
