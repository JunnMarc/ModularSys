using ModularSys.Data.Common.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync(bool includeDeleted = false);
        Task<Category?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id, string deletedBy);
        Task<bool> RestoreAsync(int id, string restoredBy);
    }
}
