using ModularSys.Data.Common.Entities.CRM;
using ModularSys.CRM.Models;

namespace ModularSys.CRM.Interface
{
    public interface IOpportunityService
    {
        Task<IEnumerable<Opportunity>> GetAllAsync(bool includeDeleted = false);
        Task<Opportunity?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(OpportunityInputModel model);
        Task UpdateAsync(OpportunityInputModel model);
        Task DeleteAsync(int id, string deletedBy = "System");
        Task<bool> RestoreAsync(int id, string restoredBy = "System");
        Task<IEnumerable<Opportunity>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Opportunity>> GetByStageAsync(string stage);
        Task<decimal> GetTotalOpportunityValueAsync();
        Task<int> GetWonOpportunitiesAsync();
        Task<decimal> GetWonOpportunityValueAsync();
    }
}
