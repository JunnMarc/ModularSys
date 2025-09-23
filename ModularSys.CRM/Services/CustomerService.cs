using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Db;
using ModularSys.CRM.Interface;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

        public CustomerService(IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Customers.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .Include(c => c.Contacts)
                .Include(c => c.Opportunities)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Customers.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .Include(c => c.Contacts)
                .Include(c => c.Opportunities)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateAsync(CustomerInputModel model)
        {
            using var context = _contextFactory.CreateDbContext();
            var customer = new Customer
            {
                CompanyName = model.CompanyName,
                ContactName = model.ContactName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                Industry = model.Industry,
                CompanySize = model.CompanySize,
                Website = model.Website,
                Notes = model.Notes,
                Status = model.Status,
                CustomerType = model.CustomerType,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerInputModel model)
        {
            if (!model.Id.HasValue) return;

            using var context = _contextFactory.CreateDbContext();
            var customer = await context.Customers.FindAsync(model.Id.Value);
            if (customer == null || customer.IsDeleted) return;

            customer.CompanyName = model.CompanyName;
            customer.ContactName = model.ContactName;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.State = model.State;
            customer.PostalCode = model.PostalCode;
            customer.Country = model.Country;
            customer.Industry = model.Industry;
            customer.CompanySize = model.CompanySize;
            customer.Website = model.Website;
            customer.Notes = model.Notes;
            customer.Status = model.Status;
            customer.CustomerType = model.CustomerType;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = "System";

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var customer = await context.Customers.FindAsync(id);
            if (customer == null || customer.IsDeleted) return;

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = deletedBy;

            await context.SaveChangesAsync();
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy = "System")
        {
            using var context = _contextFactory.CreateDbContext();
            var customer = await context.Customers.FindAsync(id);
            if (customer == null || !customer.IsDeleted) return false;

            customer.IsDeleted = false;
            customer.DeletedAt = null;
            customer.DeletedBy = null;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = restoredBy;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            using var context = _contextFactory.CreateDbContext();
            return await context.Customers
                .Where(c => !c.IsDeleted && 
                           (c.CompanyName.Contains(searchTerm) ||
                            c.ContactName.Contains(searchTerm) ||
                            c.Email.Contains(searchTerm) ||
                            (c.Phone != null && c.Phone.Contains(searchTerm))))
                .Include(c => c.Contacts)
                .Include(c => c.Opportunities)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Customers
                .Where(c => !c.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetActiveCustomersAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Customers
                .Where(c => !c.IsDeleted && c.Status == "Active")
                .CountAsync();
        }
    }
}
