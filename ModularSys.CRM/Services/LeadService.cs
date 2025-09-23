using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Services
{
    public class LeadService : ILeadService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public LeadService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Lead>> GetAllAsync(bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Leads.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(l => !l.IsDeleted);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Lead?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Leads.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(l => !l.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task CreateAsync(LeadInputModel model)
        {
            using var context = _contextFactory.CreateDbContext();
            var lead = new Lead
            {
                CompanyName = model.CompanyName,
                ContactName = model.ContactName,
                Email = model.Email,
                Phone = model.Phone,
                LeadSource = model.LeadSource,
                Industry = model.Industry,
                EstimatedValue = model.EstimatedValue,
                Notes = model.Notes,
                Status = model.Status,
                Priority = model.Priority,
                FollowUpDate = model.FollowUpDate,
                AssignedTo = model.AssignedTo,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Leads.Add(lead);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LeadInputModel model)
        {
            if (!model.Id.HasValue) return;

            using var context = _contextFactory.CreateDbContext();
            var lead = await context.Leads.FindAsync(model.Id.Value);
            if (lead == null || lead.IsDeleted) return;

            lead.CompanyName = model.CompanyName;
            lead.ContactName = model.ContactName;
            lead.Email = model.Email;
            lead.Phone = model.Phone;
            lead.LeadSource = model.LeadSource;
            lead.Industry = model.Industry;
            lead.EstimatedValue = model.EstimatedValue;
            lead.Notes = model.Notes;
            lead.Status = model.Status;
            lead.Priority = model.Priority;
            lead.FollowUpDate = model.FollowUpDate;
            lead.AssignedTo = model.AssignedTo;
            lead.UpdatedAt = DateTime.UtcNow;
            lead.UpdatedBy = "System";

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var lead = await context.Leads.FindAsync(id);
            if (lead == null || lead.IsDeleted) return;

            lead.IsDeleted = true;
            lead.DeletedAt = DateTime.UtcNow;
            lead.DeletedBy = deletedBy;

            await context.SaveChangesAsync();
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var lead = await context.Leads.FindAsync(id);
            if (lead == null || !lead.IsDeleted) return false;

            lead.IsDeleted = false;
            lead.DeletedAt = null;
            lead.DeletedBy = null;
            lead.UpdatedAt = DateTime.UtcNow;
            lead.UpdatedBy = restoredBy;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Lead>> GetByStatusAsync(string status)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Leads
                .Where(l => !l.IsDeleted && l.Status == status)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalLeadsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Leads
                .Where(l => !l.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetQualifiedLeadsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Leads
                .Where(l => !l.IsDeleted && l.Status == "Qualified")
                .CountAsync();
        }

        public async Task ConvertToCustomerAsync(int leadId, string convertedBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var lead = await context.Leads.FindAsync(leadId);
            if (lead == null || lead.IsDeleted) return;

            // Create customer from lead
            var customer = new Customer
            {
                CompanyName = lead.CompanyName,
                ContactName = lead.ContactName,
                Email = lead.Email,
                Phone = lead.Phone,
                Industry = lead.Industry,
                Notes = lead.Notes,
                Status = "Active",
                CustomerType = "Customer",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = convertedBy
            };

            context.Customers.Add(customer);

            // Update lead status
            lead.Status = "Converted";
            lead.UpdatedAt = DateTime.UtcNow;
            lead.UpdatedBy = convertedBy;

            await context.SaveChangesAsync();
        }
    }
}
