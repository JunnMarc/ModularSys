using ModularSys.Data.Common.Entities.CRM;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync(bool includeDeleted = false);
        Task<Customer?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(CustomerInputModel model);
        Task UpdateAsync(CustomerInputModel model);
        Task DeleteAsync(int id, string deletedBy = "System");
        Task<bool> RestoreAsync(int id, string restoredBy = "System");
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
        Task<int> GetTotalCustomersAsync();
        Task<int> GetActiveCustomersAsync();
    }
}
