using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Helpdesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModularSys.Helpdesk.Services
{
    public class TicketService : ITicketService
    {
        private readonly ModularSysDbContext _context;
        private readonly ISLAService _slaService;
        private readonly INotificationService _notificationService;
        private readonly IWorkflowService _workflowService;

        public TicketService(ModularSysDbContext context, ISLAService slaService, INotificationService notificationService, IWorkflowService workflowService)
        {
            _context = context;
            _slaService = slaService;
            _notificationService = notificationService;
            _workflowService = workflowService;
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = ticket.Status ?? "Open";
            ticket.Priority = ticket.Priority ?? "Medium";

            // SLA Calculation
            var dueDates = _slaService.CalculateDueDates(ticket.Priority, ticket.CreatedAt.Value);
            ticket.FirstResponseDueAt = dueDates.FirstResponseDue;
            ticket.ResolutionDueAt = dueDates.ResolutionDue;

            // WORKFLOW: Auto-Assign
            await _workflowService.ProcessTicketCreationAsync(ticket);
            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // NOTIFICATION
            // Try access customer email via relation if loaded, else skip or fetch
            // For MVP assuming passed ticket has Customer email or we look it up? 
            // The ticket object usually just has CustomerId. We need to load Customer if not present.
            if (ticket.CustomerId > 0)
            {
               var customer = await _context.Customers.FindAsync(ticket.CustomerId);
               if (customer != null && !string.IsNullOrEmpty(customer.Email))
               {
                   try 
                   {
                       await _notificationService.SendTicketCreatedNotificationAsync(customer.Email, ticket.Subject, ticket.Id);
                   }
                   catch (Exception ex)
                   {
                       System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
                   }
               }
            }

            return ticket;
        }

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Comments)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(System.Security.Claims.ClaimsPrincipal user, int? customerId = null, string? status = null, int? assignedToId = null)
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .AsQueryable();

            // SECURITY: Row-Level Security
            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                // Force filter to CURRENT customer only. Ignore the parameter.
                var claimId = user.FindFirst("CustomerCustomerId")?.Value ?? user.FindFirst("CustomerId")?.Value;
                if (int.TryParse(claimId, out int myCustomerId))
                {
                    query = query.Where(t => t.CustomerId == myCustomerId);
                }
                else
                {
                    // Authenticated as Customer but no ID? Return nothing for safety.
                    return Enumerable.Empty<Ticket>();
                }
            }
            else
            {
                // Admin/Agent/Staff: Allow filtering as requested
                if (customerId.HasValue)
                    query = query.Where(t => t.CustomerId == customerId);
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<TicketComment> AddCommentAsync(int ticketId, string content, bool isInternal, string author)
        {
            var comment = new TicketComment
            {
                TicketId = ticketId,
                Content = content,
                IsInternal = isInternal,
                AuthorName = author,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task AssignTicketAsync(int ticketId, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.AssignedToId = userId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStatusAsync(int ticketId, string status)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.Status = status;
                if (status == "Resolved" && ticket.ResolvedAt == null)
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, int>> GetTicketCountsByStatusAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetTicketCountsByPriorityAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);
        }
    }
}
