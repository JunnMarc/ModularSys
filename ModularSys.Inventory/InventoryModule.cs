using Microsoft.Extensions.DependencyInjection;
using ModularSys.Core.Interfaces;
using ModularSys.Inventory.Interface;
using ModularSys.Inventory.Services;
using MudBlazor;

public class InventoryModule : ISubsystem
{
    public string Name => "Inventory";
    public string Route => "/inventory";
    public string? Icon => Icons.Material.Outlined.Inventory;
    public int Order => 2;

    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IRevenueService, RevenueService>();
        services.AddScoped<IInventoryDashboardService, InventoryDashboardService>();
        services.AddScoped<IBusinessAnalyticsService, BusinessAnalyticsService>();
        services.AddScoped<IAccountingReportService, AccountingReportService>();
        services.AddScoped<IAnalyticalReportService, AnalyticalReportService>();
    }
}
