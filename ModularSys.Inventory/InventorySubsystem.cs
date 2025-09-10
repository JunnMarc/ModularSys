using Microsoft.Extensions.DependencyInjection;
using ModularSys.Inventory.Services;
using ModularSys.Core.Interfaces;
using MudBlazor;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

public class InventorySubsystem : ISubsystem
{
    public string Name => "Inventory";
    public string Route => "/inventory";
    public string? Icon => Icons.Material.Outlined.Inventory;
    public int Order => 2;

    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
    } 

}