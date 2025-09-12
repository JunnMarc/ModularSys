using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Interface
{
    public interface IRevenueService
    {
        Task RecordAsync(decimal amount, string source, string reference); // standalone
        Task RecordAsync(InventoryDbContext db, decimal amount, string source, string reference); // transactional
        Task<decimal> GetTotalAsync();
        Task<IEnumerable<RevenueTransaction>> GetAllAsync();
    }

}
