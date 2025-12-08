using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModularSys.Data.Common.Entities.Helpdesk;

namespace ModularSys.Core.Interfaces
{
    public interface ITicketService
    {
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketAsync(Ticket ticket);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsAsync(int? customerId = null, string? status = null, int? assignedToId = null);
        Task<TicketComment> AddCommentAsync(int ticketId, string content, bool isInternal, string author);
        Task AssignTicketAsync(int ticketId, int userId);
        Task UpdateStatusAsync(int ticketId, string status);
        
        // Dashboard Stats
        Task<Dictionary<string, int>> GetTicketCountsByStatusAsync();
        Task<Dictionary<string, int>> GetTicketCountsByPriorityAsync();
    }
}
