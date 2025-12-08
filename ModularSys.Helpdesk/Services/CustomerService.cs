using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.CRM;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModularSys.Helpdesk.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ModularSysDbContext _context;

        public CustomerService(ModularSysDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersBySegmentAsync(string segment)
        {
            return await _context.Customers
                .Where(c => !c.IsDeleted && c.SegmentationTag == segment)
                .ToListAsync();
        }
    }
}
