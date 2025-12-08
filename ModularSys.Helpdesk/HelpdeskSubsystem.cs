using Microsoft.Extensions.DependencyInjection;
using ModularSys.Core.Interfaces;
using ModularSys.Helpdesk.Services;

namespace ModularSys.Helpdesk
{
    public class HelpdeskSubsystem : ISubsystem
    {
        public string Name => "Helpdesk";
        public string Route => "/helpdesk/tickets"; // Default route
        public string Icon => "support_agent";
        public int Order => 2;

        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISLAService, SLAService>();
            services.AddScoped<INotificationService, EmailNotificationService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<HelpdeskNavigationService>();
        }
    }
}
