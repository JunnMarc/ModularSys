using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularSys.Core.DTOs;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Helpdesk;
using System.Security.Claims;

namespace ModularSys.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require login by default
    public class TicketController : ControllerBase
    {
        private readonly ModularSysDbContext _context;

        public TicketController(ModularSysDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetMyTickets()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdStr))
                return BadRequest("User is not linked to a customer account.");

            if (!int.TryParse(customerIdStr, out int customerId))
                return BadRequest("Invalid customer ID.");

            var tickets = await _context.Tickets
                .Where(t => t.CustomerId == customerId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    Subject = t.Subject,
                    Description = t.Description ?? string.Empty,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
                return BadRequest("User not linked to customer.");

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == id && t.CustomerId == customerId && !t.IsDeleted);

            if (ticket == null) return NotFound();

            return Ok(new TicketDto 
            {
                Id = ticket.Id,
                Subject = ticket.Subject,
                Description = ticket.Description ?? string.Empty,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt ?? DateTime.MinValue
            });
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] TicketDto request)
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
                return BadRequest("User not linked to customer.");

            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Subject)) return BadRequest("Subject required");

            // Look up Default category or allow selection (simplifying to 'General')
            var category = await _context.TicketCategories.FirstOrDefaultAsync() 
                           ?? new TicketCategory { Name = "General", Description = "Default" }; // Should rely on seed data

            var ticket = new Ticket
            {
                Subject = request.Subject,
                Description = request.Description ?? string.Empty,
                CustomerId = customerId,
                CategoryId = category.Id,
                Priority = "Normal", // Default
                Status = "Open",
                Channel = "Web",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            request.Id = ticket.Id;
            request.Status = ticket.Status;
            request.Priority = ticket.Priority;
            request.CreatedAt = ticket.CreatedAt ?? DateTime.MinValue;

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, request);
        }
    }
}
