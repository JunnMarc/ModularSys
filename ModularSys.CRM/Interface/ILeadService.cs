using ModularSys.Data.Common.Entities.CRM;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface ILeadService
    {
        Task<IEnumerable<Lead>> GetAllAsync(bool includeDeleted = false);
        Task<Lead?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(LeadInputModel model);
        Task UpdateAsync(LeadInputModel model);
        Task DeleteAsync(int id, string deletedBy = "System");
        Task<bool> RestoreAsync(int id, string restoredBy = "System");
        Task<IEnumerable<Lead>> GetByStatusAsync(string status);
        Task<int> GetTotalLeadsAsync();
        Task<int> GetQualifiedLeadsAsync();
        Task ConvertToCustomerAsync(int leadId, string convertedBy = "System");
    }
}
