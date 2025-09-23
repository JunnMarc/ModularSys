using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public OpportunityService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Opportunity>> GetAllAsync(bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Opportunities.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(o => !o.IsDeleted);
            }

            return await query
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Opportunity?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Opportunities.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(o => !o.IsDeleted);
            }

            return await query
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task CreateAsync(OpportunityInputModel model)
        {
            using var context = _contextFactory.CreateDbContext();
            var opportunity = new Opportunity
            {
                CustomerId = model.CustomerId,
                Name = model.Name,
                Description = model.Description,
                Value = model.Value,
                Stage = model.Stage,
                Probability = model.Probability,
                ExpectedCloseDate = model.ExpectedCloseDate,
                LeadSource = model.LeadSource,
                Competitor = model.Competitor,
                Notes = model.Notes,
                AssignedTo = model.AssignedTo,
                Priority = model.Priority,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Opportunities.Add(opportunity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OpportunityInputModel model)
        {
            if (!model.Id.HasValue) return;

            using var context = _contextFactory.CreateDbContext();
            var opportunity = await context.Opportunities.FindAsync(model.Id.Value);
            if (opportunity == null || opportunity.IsDeleted) return;

            opportunity.CustomerId = model.CustomerId;
            opportunity.Name = model.Name;
            opportunity.Description = model.Description;
            opportunity.Value = model.Value;
            opportunity.Stage = model.Stage;
            opportunity.Probability = model.Probability;
            opportunity.ExpectedCloseDate = model.ExpectedCloseDate;
            opportunity.LeadSource = model.LeadSource;
            opportunity.Competitor = model.Competitor;
            opportunity.Notes = model.Notes;
            opportunity.AssignedTo = model.AssignedTo;
            opportunity.Priority = model.Priority;
            opportunity.UpdatedAt = DateTime.UtcNow;
            opportunity.UpdatedBy = "System";

            // Set actual close date if stage is Won or Lost
            if (model.Stage == "Won" || model.Stage == "Lost")
            {
                if (!opportunity.ActualCloseDate.HasValue)
                {
                    opportunity.ActualCloseDate = DateTime.UtcNow;
                }
            }
            else
            {
                opportunity.ActualCloseDate = null;
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var opportunity = await context.Opportunities.FindAsync(id);
            if (opportunity == null || opportunity.IsDeleted) return;

            opportunity.IsDeleted = true;
            opportunity.DeletedAt = DateTime.UtcNow;
            opportunity.DeletedBy = deletedBy;

            await context.SaveChangesAsync();
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var opportunity = await context.Opportunities.FindAsync(id);
            if (opportunity == null || !opportunity.IsDeleted) return false;

            opportunity.IsDeleted = false;
            opportunity.DeletedAt = null;
            opportunity.DeletedBy = null;
            opportunity.UpdatedAt = DateTime.UtcNow;
            opportunity.UpdatedBy = restoredBy;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Opportunity>> GetByCustomerIdAsync(int customerId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Opportunities
                .Where(o => !o.IsDeleted && o.CustomerId == customerId)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Opportunity>> GetByStageAsync(string stage)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == stage)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalOpportunityValueAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Opportunities
                .Where(o => !o.IsDeleted)
                .SumAsync(o => o.Value);
        }

        public async Task<int> GetWonOpportunitiesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == "Won")
                .CountAsync();
        }

        public async Task<decimal> GetWonOpportunityValueAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == "Won")
                .SumAsync(o => o.Value);
        }
    }
}
