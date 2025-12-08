using System.Collections.Generic;
using System.Threading.Tasks;
using ModularSys.Data.Common.Entities.CRM;

namespace ModularSys.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task UpdateCustomerAsync(Customer customer);
        Task<IEnumerable<Customer>> GetCustomersBySegmentAsync(string segment);
    }
}
