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

        public TicketService(ModularSysDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            // SLA Calculation (Basic)
            // Ideally we fetch the SLA based on Priority or Category
            // For now, hardcode or fetch default
            
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = ticket.Status ?? "Open";
            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
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

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int? customerId = null, string? status = null, int? assignedToId = null)
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(t => t.CustomerId == customerId);

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
