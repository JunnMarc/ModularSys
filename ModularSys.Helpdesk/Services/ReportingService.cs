using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModularSys.Helpdesk.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ModularSysDbContext _context;

        public ReportingService(ModularSysDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, double>> GetCustomerSatisfactionStatsAsync()
        {
            // Dummy logic for now since we haven't implemented comprehensive CSAT collection yet
            // Assuming Customer.SatisfactionScore exists
            return await Task.FromResult(new Dictionary<string, double>
            {
                { "Average", 4.5 },
                { "NPS", 72 }
            });
        }

        public async Task<Dictionary<string, int>> GetTicketsByChannelStatsAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Channel)
                .Select(g => new { Channel = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Channel, x => x.Count);
        }

        public async Task<double> GetAverageResolutionTimeAsync()
        {
            var resolvedTickets = await _context.Tickets
                .Where(t => t.ResolvedAt != null && t.CreatedAt != null)
                .Select(t => new { Created = t.CreatedAt, Resolved = t.ResolvedAt })
                .ToListAsync();

            if (!resolvedTickets.Any()) return 0;

            return resolvedTickets.Average(t => (t.Resolved!.Value - t.Created!.Value).TotalHours);
        }

        public async Task<int> GetSLABreachCountAsync()
        {
            return await _context.Tickets.CountAsync(t => t.ResolutionBreached || t.FirstResponseBreached);
        }
    }
}
