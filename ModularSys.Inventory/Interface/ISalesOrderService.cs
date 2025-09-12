using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Inventory.Interface
{
    public interface ISalesOrderService
    {
        Task<int> CreateAsync(SalesOrder order);
        Task CompleteAsync(int salesOrderId);
        Task<SalesOrder?> GetByIdAsync(int id);
        Task<IEnumerable<SalesOrder>> GetAllAsync();
    }
}
