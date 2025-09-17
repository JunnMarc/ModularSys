using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;

namespace ModularSys.Inventory.Services
{
    public class AutoPurchaseService : IAutoPurchaseService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoPurchaseService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Scans for low-stock revenue-critical items and prepares a PO without saving.
        /// </summary>
        public async Task<PurchaseOrder> PrepareLowStockPurchaseOrderAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var lowStockItems = await db.Products
                .Include(p => p.Category)
                .Where(p => p.QuantityOnHand < p.ReorderLevel && p.Category.IsRevenueCritical)
                .ToListAsync();

            var po = new PurchaseOrder
            {
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Lines = new List<PurchaseOrderLine>()
            };

            foreach (var item in lowStockItems)
            {
                var qtyToOrder = item.ReorderLevel * 2; // Example rule
                po.Lines.Add(new PurchaseOrderLine
                {
                    ProductId = item.ProductId,
                    Quantity = qtyToOrder,
                    UnitCost = item.CostPrice
                });
            }

            po.SubTotal = po.Lines.Sum(l => l.LineTotal);
            return po; // Not saved yet — UI can display for confirmation
        }

        /// <summary>
        /// Saves the prepared PO and logs inventory transactions.
        /// </summary>
        public async Task<int> ConfirmPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            // Attach products to context to avoid duplicate tracking
            foreach (var line in purchaseOrder.Lines)
            {
                var product = await db.Products.FindAsync(line.ProductId);
                if (product != null)
                {
                    db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = product.ProductId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = "PurchaseOrder",
                        QuantityChange = line.Quantity,
                        Amount = -(line.Quantity * line.UnitCost)
                    });
                }
            }

            db.PurchaseOrders.Add(purchaseOrder);
            return await db.SaveChangesAsync();
        }
    }
}
