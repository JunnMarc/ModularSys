using ModularSys.Data.Common.Entities.CRM;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetAllAsync(bool includeDeleted = false);
        Task<Contact?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(ContactInputModel model);
        Task UpdateAsync(ContactInputModel model);
        Task DeleteAsync(int id, string deletedBy = "System");
        Task<bool> RestoreAsync(int id, string restoredBy = "System");
        Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId);
        Task<int> GetTotalContactsAsync();
    }
}
