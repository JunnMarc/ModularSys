using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Entities.Inventory;
using System.Text.Json;

namespace ModularSys.Inventory.Services
{
    /// <summary>
    /// Modular audit logging service for Inventory operations.
    /// Plug-and-play: Works independently if IAuditService is available, silently fails if not.
    /// </summary>
    public class InventoryAuditService
    {
        private readonly Lazy<IAuditService?> _auditService;
        private readonly IAuthService _authService;

        public InventoryAuditService(IServiceProvider serviceProvider, IAuthService authService)
        {
            _authService = authService;
            _auditService = new Lazy<IAuditService?>(() =>
            {
                try
                {
                    return serviceProvider.GetService(typeof(IAuditService)) as IAuditService;
                }
                catch
                {
                    return null; // Audit service not available
                }
            });
        }

        #region Product Audit Logs

        public async Task LogProductCreatedAsync(Product product)
        {
            await LogAsync("Create", "Product", product.ProductId.ToString(),
                newValues: new
                {
                    product.SKU,
                    product.Name,
                    product.CategoryId,
                    product.UnitPrice,
                    product.QuantityOnHand,
                    product.ReorderLevel
                },
                description: $"Created product '{product.Name}' (SKU: {product.SKU})");
        }

        public async Task LogProductUpdatedAsync(Product oldProduct, Product newProduct)
        {
            await LogAsync("Update", "Product", newProduct.ProductId.ToString(),
                oldValues: new
                {
                    oldProduct.SKU,
                    oldProduct.Name,
                    oldProduct.CategoryId,
                    oldProduct.UnitPrice,
                    oldProduct.QuantityOnHand,
                    oldProduct.ReorderLevel,
                    oldProduct.IsActive
                },
                newValues: new
                {
                    newProduct.SKU,
                    newProduct.Name,
                    newProduct.CategoryId,
                    newProduct.UnitPrice,
                    newProduct.QuantityOnHand,
                    newProduct.ReorderLevel,
                    newProduct.IsActive
                },
                description: $"Updated product '{newProduct.Name}' (SKU: {newProduct.SKU})");
        }

        public async Task LogProductDeletedAsync(Product product)
        {
            await LogAsync("Delete", "Product", product.ProductId.ToString(),
                oldValues: new
                {
                    product.SKU,
                    product.Name,
                    product.CategoryId,
                    product.UnitPrice,
                    product.QuantityOnHand
                },
                description: $"Deleted product '{product.Name}' (SKU: {product.SKU})");
        }

        public async Task LogProductRestoredAsync(Product product)
        {
            await LogAsync("Restore", "Product", product.ProductId.ToString(),
                description: $"Restored product '{product.Name}' (SKU: {product.SKU})");
        }

        #endregion

        #region Category Audit Logs

        public async Task LogCategoryCreatedAsync(Category category)
        {
            await LogAsync("Create", "Category", category.CategoryId.ToString(),
                newValues: new { category.CategoryName, category.Description },
                description: $"Created category '{category.CategoryName}'");
        }

        public async Task LogCategoryUpdatedAsync(Category oldCategory, Category newCategory)
        {
            await LogAsync("Update", "Category", newCategory.CategoryId.ToString(),
                oldValues: new { oldCategory.CategoryName, oldCategory.Description },
                newValues: new { newCategory.CategoryName, newCategory.Description },
                description: $"Updated category '{newCategory.CategoryName}'");
        }

        public async Task LogCategoryDeletedAsync(Category category)
        {
            await LogAsync("Delete", "Category", category.CategoryId.ToString(),
                oldValues: new { category.CategoryName, category.Description },
                description: $"Deleted category '{category.CategoryName}'");
        }

        #endregion

        #region Sales Order Audit Logs

        public async Task LogSalesOrderCreatedAsync(SalesOrder order)
        {
            await LogAsync("Create", "SalesOrder", order.SalesOrderId.ToString(),
                newValues: new
                {
                    order.OrderNumber,
                    order.CustomerName,
                    order.TotalAmount,
                    order.Status,
                    ItemCount = order.Lines?.Count ?? 0
                },
                description: $"Created sales order {order.OrderNumber} for {order.CustomerName} (Total: {order.TotalAmount:C})");
        }

        public async Task LogSalesOrderUpdatedAsync(SalesOrder oldOrder, SalesOrder newOrder)
        {
            await LogAsync("Update", "SalesOrder", newOrder.SalesOrderId.ToString(),
                oldValues: new { oldOrder.Status, oldOrder.TotalAmount },
                newValues: new { newOrder.Status, newOrder.TotalAmount },
                description: $"Updated sales order {newOrder.OrderNumber}");
        }

        public async Task LogSalesOrderStatusChangedAsync(SalesOrder order, string oldStatus, string newStatus)
        {
            await LogAsync("StatusChange", "SalesOrder", order.SalesOrderId.ToString(),
                oldValues: new { Status = oldStatus },
                newValues: new { Status = newStatus },
                description: $"Changed sales order {order.OrderNumber} status from {oldStatus} to {newStatus}");
        }

        public async Task LogSalesOrderDeletedAsync(SalesOrder order)
        {
            await LogAsync("Delete", "SalesOrder", order.SalesOrderId.ToString(),
                oldValues: new { order.OrderNumber, order.CustomerName, order.TotalAmount },
                description: $"Deleted sales order {order.OrderNumber}");
        }

        #endregion

        #region Purchase Order Audit Logs

        public async Task LogPurchaseOrderCreatedAsync(PurchaseOrder order)
        {
            await LogAsync("Create", "PurchaseOrder", order.PurchaseOrderId.ToString(),
                newValues: new
                {
                    order.OrderNumber,
                    order.SupplierName,
                    order.TotalAmount,
                    order.Status,
                    ItemCount = order.Lines?.Count ?? 0
                },
                description: $"Created purchase order {order.OrderNumber} from {order.SupplierName} (Total: {order.TotalAmount:C})");
        }

        public async Task LogPurchaseOrderUpdatedAsync(PurchaseOrder oldOrder, PurchaseOrder newOrder)
        {
            await LogAsync("Update", "PurchaseOrder", newOrder.PurchaseOrderId.ToString(),
                oldValues: new { oldOrder.Status, oldOrder.TotalAmount },
                newValues: new { newOrder.Status, newOrder.TotalAmount },
                description: $"Updated purchase order {newOrder.OrderNumber}");
        }

        public async Task LogPurchaseOrderStatusChangedAsync(PurchaseOrder order, string oldStatus, string newStatus)
        {
            await LogAsync("StatusChange", "PurchaseOrder", order.PurchaseOrderId.ToString(),
                oldValues: new { Status = oldStatus },
                newValues: new { Status = newStatus },
                description: $"Changed purchase order {order.OrderNumber} status from {oldStatus} to {newStatus}");
        }

        public async Task LogPurchaseOrderDeletedAsync(PurchaseOrder order)
        {
            await LogAsync("Delete", "PurchaseOrder", order.PurchaseOrderId.ToString(),
                oldValues: new { order.OrderNumber, order.SupplierName, order.TotalAmount },
                description: $"Deleted purchase order {order.OrderNumber}");
        }

        #endregion

        #region Inventory Transaction Audit Logs

        public async Task LogInventoryAdjustmentAsync(InventoryTransaction transaction)
        {
            await LogAsync("InventoryAdjustment", "InventoryTransaction", transaction.InventoryTransactionId.ToString(),
                newValues: new
                {
                    transaction.ProductId,
                    transaction.TransactionType,
                    transaction.QuantityChange,
                    transaction.Reason
                },
                description: $"Inventory adjustment: {transaction.TransactionType} - {transaction.QuantityChange} units (Reason: {transaction.Reason})");
        }

        public async Task LogStockMovementAsync(int productId, string productName, int quantityChange, string reason)
        {
            await LogAsync("StockMovement", "Product", productId.ToString(),
                newValues: new { QuantityChange = quantityChange, Reason = reason },
                description: $"Stock movement for '{productName}': {(quantityChange > 0 ? "+" : "")}{quantityChange} units ({reason})");
        }

        #endregion

        #region Helper Methods

        private async Task LogAsync(string action, string entityName, string entityId,
            object? oldValues = null, object? newValues = null, string? description = null)
        {
            if (_auditService.Value == null)
                return; // Audit service not available, silently skip

            try
            {
                var auditLog = new ModularSys.Data.Common.Entities.AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    Description = description,
                    Category = "Inventory",
                    Severity = action == "Delete" ? "Warning" : "Info",
                    Username = _authService.CurrentUser ?? "System",
                    Timestamp = DateTime.UtcNow,
                    Success = true
                };

                await _auditService.Value.LogAsync(auditLog);
            }
            catch
            {
                // Silently fail - don't break inventory operations if audit logging fails
            }
        }

        #endregion
    }
}
