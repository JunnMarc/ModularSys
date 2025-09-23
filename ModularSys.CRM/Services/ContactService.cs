using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Services
{
    public class ContactService : IContactService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public ContactService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Contact>> GetAllAsync(bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Contacts.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .Include(c => c.Customer)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Contacts.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateAsync(ContactInputModel model)
        {
            using var context = _contextFactory.CreateDbContext();
            var contact = new Contact
            {
                CustomerId = model.CustomerId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                JobTitle = model.JobTitle,
                Department = model.Department,
                IsPrimary = model.IsPrimary,
                Notes = model.Notes,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ContactInputModel model)
        {
            if (!model.Id.HasValue) return;

            using var context = _contextFactory.CreateDbContext();
            var contact = await context.Contacts.FindAsync(model.Id.Value);
            if (contact == null || contact.IsDeleted) return;

            contact.CustomerId = model.CustomerId;
            contact.FirstName = model.FirstName;
            contact.LastName = model.LastName;
            contact.Email = model.Email;
            contact.Phone = model.Phone;
            contact.JobTitle = model.JobTitle;
            contact.Department = model.Department;
            contact.IsPrimary = model.IsPrimary;
            contact.Notes = model.Notes;
            contact.Status = model.Status;
            contact.UpdatedAt = DateTime.UtcNow;
            contact.UpdatedBy = "System";

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var contact = await context.Contacts.FindAsync(id);
            if (contact == null || contact.IsDeleted) return;

            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;
            contact.DeletedBy = deletedBy;

            await context.SaveChangesAsync();
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var contact = await context.Contacts.FindAsync(id);
            if (contact == null || !contact.IsDeleted) return false;

            contact.IsDeleted = false;
            contact.DeletedAt = null;
            contact.DeletedBy = null;
            contact.UpdatedAt = DateTime.UtcNow;
            contact.UpdatedBy = restoredBy;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Contacts
                .Where(c => !c.IsDeleted && c.CustomerId == customerId)
                .Include(c => c.Customer)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<int> GetTotalContactsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Contacts
                .Where(c => !c.IsDeleted)
                .CountAsync();
        }
    }
}
