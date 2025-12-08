using System;

namespace ModularSys.Helpdesk.Services
{
    public class SLAService : ISLAService
    {
        // Standard SLA Definitions (in Hours)
        // Ideally effectively loaded from DB/Config, but hardcoded for MVP
        private static readonly Dictionary<string, (double ResponseHours, double ResolutionHours)> _slaPolicies = new()
        {
            { "Critical", (1, 4) },
            { "High", (4, 8) },
            { "Medium", (8, 24) },
            { "Low", (24, 48) }
        };

        public (DateTime FirstResponseDue, DateTime ResolutionDue) CalculateDueDates(string priority, DateTime createdAt)
        {
            // Default to Medium if unknown
            if (!_slaPolicies.TryGetValue(priority ?? "Medium", out var policy))
            {
                policy = _slaPolicies["Medium"];
            }

            // Simple calculation (24/7 coverage assumed for simplicity in MVP Phase 1)
            // TODO: Implement Business Hours Logic (Mon-Fri 9-5) in Phase 2
            
            var responseDue = createdAt.AddHours(policy.ResponseHours);
            var resolutionDue = createdAt.AddHours(policy.ResolutionHours);

            return (responseDue, resolutionDue);
        }

        public bool CheckFirstResponseBreach(DateTime dueAt, DateTime? actualAt)
        {
            if (actualAt.HasValue)
            {
                return actualAt.Value > dueAt;
            }
            return DateTime.UtcNow > dueAt;
        }

        public bool CheckResolutionBreach(DateTime dueAt, DateTime? actualAt)
        {
            if (actualAt.HasValue)
            {
                return actualAt.Value > dueAt;
            }
            return DateTime.UtcNow > dueAt;
        }
    }
}
