# Inventory Module - Modular Audit Logging

## Overview

This module implements a **plug-and-play audit logging system** that is completely optional and modular. The inventory module will work perfectly fine with or without the audit logging system.

## Key Features

✅ **Plug-and-Play**: Works automatically if `IAuditService` is available, silently skips if not  
✅ **Zero Breaking Changes**: Existing code continues to work without modifications  
✅ **No Dependencies**: Inventory module doesn't require audit logging to function  
✅ **Graceful Degradation**: Audit logging failures don't break inventory operations  
✅ **Comprehensive Logging**: Tracks all CRUD operations on Products, Categories, Orders, and Inventory Transactions

## Architecture

### 1. **InventoryAuditService**
Location: `Services/InventoryAuditService.cs`

This service provides audit logging methods for all inventory entities:
- Products (Create, Update, Delete, Restore)
- Categories (Create, Update, Delete)
- Sales Orders (Create, Update, Status Change, Delete)
- Purchase Orders (Create, Update, Status Change, Delete)
- Inventory Transactions (Adjustments, Stock Movements)

**Key Design Principles:**
- Uses `Lazy<IAuditService?>` to avoid circular dependencies
- Returns `null` if audit service is not available
- All methods are wrapped in try-catch to prevent failures
- Automatically categorizes logs as "Inventory"

### 2. **Service Integration**
Example: `ProductService.cs`

```csharp
public class ProductService : IProductService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InventoryAuditService? _auditService; // Optional!

    public ProductService(IServiceScopeFactory scopeFactory, InventoryAuditService? auditService = null)
    {
        _scopeFactory = scopeFactory;
        _auditService = auditService; // Works without it
    }

    public async Task CreateAsync(ProductInputModel model)
    {
        // ... create product logic ...
        
        // Audit log (plug-and-play)
        if (_auditService != null)
            await _auditService.LogProductCreatedAsync(entity);
    }
}
```

### 3. **Module Registration**
Location: `InventoryModule.cs`

```csharp
public void RegisterServices(IServiceCollection services)
{
    // Core services...
    
    // Modular Audit Logging (Plug-and-Play)
    services.AddScoped<InventoryAuditService>();
}
```

## Usage

### Automatic Logging

Once integrated, audit logs are automatically created for:

**Products:**
- ✅ Product created
- ✅ Product updated (with old/new values)
- ✅ Product deleted (soft delete)
- ✅ Product restored

**Categories:**
- ✅ Category created
- ✅ Category updated
- ✅ Category deleted

**Sales Orders:**
- ✅ Order created
- ✅ Order updated
- ✅ Order status changed
- ✅ Order deleted

**Purchase Orders:**
- ✅ Order created
- ✅ Order updated
- ✅ Order status changed
- ✅ Order deleted

**Inventory Transactions:**
- ✅ Inventory adjustments
- ✅ Stock movements

### Manual Logging

You can also use the audit service directly:

```csharp
// In any inventory service
if (_auditService != null)
{
    await _auditService.LogStockMovementAsync(
        productId: 123,
        productName: "Widget",
        quantityChange: -50,
        reason: "Damaged goods"
    );
}
```

## Integration with Other Services

### CategoryService Example

```csharp
public class CategoryService : ICategoryService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InventoryAuditService? _auditService;

    public CategoryService(IServiceScopeFactory scopeFactory, InventoryAuditService? auditService = null)
    {
        _scopeFactory = scopeFactory;
        _auditService = auditService;
    }

    public async Task CreateAsync(CategoryInputModel model)
    {
        // ... create category ...
        
        if (_auditService != null)
            await _auditService.LogCategoryCreatedAsync(entity);
    }
}
```

### SalesOrderService Example

```csharp
public class SalesOrderService : ISalesOrderService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InventoryAuditService? _auditService;

    public SalesOrderService(IServiceScopeFactory scopeFactory, InventoryAuditService? auditService = null)
    {
        _scopeFactory = scopeFactory;
        _auditService = auditService;
    }

    public async Task UpdateStatusAsync(int orderId, string newStatus)
    {
        var order = await GetOrderAsync(orderId);
        var oldStatus = order.Status;
        
        // ... update status ...
        
        if (_auditService != null)
            await _auditService.LogSalesOrderStatusChangedAsync(order, oldStatus, newStatus);
    }
}
```

## Viewing Audit Logs

Navigate to **Audit Logs** page (`/audit-logs`) in the main application.

Filter by:
- **Category**: "Inventory"
- **Entity**: Product, Category, SalesOrder, PurchaseOrder, etc.
- **Action**: Create, Update, Delete, StatusChange, etc.

## Benefits

### 1. **Modularity**
- Inventory module is self-contained
- No hard dependency on audit logging
- Can be deployed with or without audit logging

### 2. **Maintainability**
- Audit logic is centralized in `InventoryAuditService`
- Easy to add new audit log types
- Consistent audit log format across all inventory operations

### 3. **Performance**
- Audit logging is asynchronous
- Failures don't impact inventory operations
- Minimal overhead

### 4. **Flexibility**
- Easy to enable/disable audit logging
- Can customize what gets logged
- Can add custom audit log fields

## Extending to Other Modules

To add audit logging to other modules (CRM, Finance, etc.):

1. **Create Module-Specific Audit Service**
```csharp
public class CrmAuditService
{
    private readonly Lazy<IAuditService?> _auditService;
    
    public CrmAuditService(IServiceProvider serviceProvider)
    {
        _auditService = new Lazy<IAuditService?>(() =>
            serviceProvider.GetService(typeof(IAuditService)) as IAuditService);
    }
    
    public async Task LogCustomerCreatedAsync(Customer customer)
    {
        if (_auditService.Value == null) return;
        
        await _auditService.Value.LogAsync(new AuditLog
        {
            Action = "Create",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            Category = "CRM",
            // ... other fields
        });
    }
}
```

2. **Register in Module**
```csharp
public class CrmModule : ISubsystem
{
    public void RegisterServices(IServiceCollection services)
    {
        // Core services...
        
        // Audit logging (plug-and-play)
        services.AddScoped<CrmAuditService>();
    }
}
```

3. **Inject into Services**
```csharp
public class CustomerService
{
    private readonly CrmAuditService? _auditService;
    
    public CustomerService(CrmAuditService? auditService = null)
    {
        _auditService = auditService;
    }
}
```

## Best Practices

1. ✅ **Always make audit service optional** (`?` nullable)
2. ✅ **Always check for null** before calling audit methods
3. ✅ **Wrap in try-catch** if needed (already done in `InventoryAuditService`)
4. ✅ **Log meaningful descriptions** for better readability
5. ✅ **Include old/new values** for update operations
6. ✅ **Use appropriate categories** (Inventory, CRM, Finance, etc.)
7. ✅ **Set correct severity** (Info, Warning, Error, Critical)

## Troubleshooting

### Audit Logs Not Appearing

1. Check that `IAuditService` is registered in `MauiProgram.cs`
2. Verify `InventoryAuditService` is registered in `InventoryModule.cs`
3. Check that the service is being injected (not null)
4. Look for exceptions in console/logs

### Performance Issues

1. Audit logging is already asynchronous
2. Failures are silently caught
3. Consider batching if logging many operations

## Future Enhancements

- [ ] Batch audit logging for bulk operations
- [ ] Configurable audit log levels (verbose, normal, minimal)
- [ ] Custom audit log fields per module
- [ ] Real-time audit log streaming
- [ ] Audit log retention policies per module

## Support

For questions or issues, refer to the main audit logging documentation at `Documents/AUDIT_LOGGING_GUIDE.md`.
