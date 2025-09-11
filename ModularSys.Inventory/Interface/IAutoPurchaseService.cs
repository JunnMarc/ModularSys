using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Inventory.Interface
{
    public interface IAutoPurchaseService
    {
        Task<PurchaseOrder> PrepareLowStockPurchaseOrderAsync();
        Task<int> ConfirmPurchaseOrderAsync(PurchaseOrder purchaseOrder);
    }
}
