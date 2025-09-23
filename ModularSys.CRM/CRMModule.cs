using Microsoft.Extensions.DependencyInjection;
using ModularSys.Core.Interfaces;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Services;
using MudBlazor;

public class CRMModule : ISubsystem
{
    public string Name => "CRM";
    public string Route => "/crm";
    public string? Icon => Icons.Material.Outlined.People;
    public int Order => 3;

    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<ICRMDashboardService, CRMDashboardService>();
        services.AddScoped<ICRMAnalyticsService, CRMAnalyticsService>();
        services.AddScoped<ICRMReportService, CRMReportService>();
    }
}
