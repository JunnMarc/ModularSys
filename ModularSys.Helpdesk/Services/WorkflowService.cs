using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Helpdesk;

namespace ModularSys.Helpdesk.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public WorkflowService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task ProcessTicketCreationAsync(Ticket ticket)
        {
            // Simple Rule Engine (In-Memory for MVP)
            // Rule 1: High/Critical Priority -> Assign to IT Admin
            // Rule 2: "Printer" or "Hardware" in Subject -> Assign to IT Support

            if (ticket.Priority == "Critical" || ticket.Priority == "High")
            {
                await AssignToRoleAsync(ticket, "Administrator");
            }
            else if (ticket.Subject.Contains("Printer", StringComparison.OrdinalIgnoreCase) || 
                     ticket.Subject.Contains("Hardware", StringComparison.OrdinalIgnoreCase))
            {
               // Example: Assign to specific department or user if we had that logic
               // For now, let's just tag it or assign to admin too as fallback
               await AssignToRoleAsync(ticket, "Administrator"); 
            }
        }

        private async Task AssignToRoleAsync(Ticket ticket, string roleName)
        {
             // Prevent overwriting if already assigned
            if (ticket.AssignedToId.HasValue) return;

            using var context = _contextFactory.CreateDbContext();
            
            // Find a user with this role
            // Ideally we pick 'Least Loaded' agent, but for MVP just pick the first one
            var agent = await context.Users
                .Where(u => u.Role.RoleName == roleName)
                .FirstOrDefaultAsync();

            if (agent != null)
            {
                ticket.AssignedToId = agent.Id;
                System.Diagnostics.Debug.WriteLine($"[Workflow] Auto-Assigned Ticket #{ticket.Id} to {agent.Username} ({roleName})");
            }
        }
    }
}
