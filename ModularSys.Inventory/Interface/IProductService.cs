using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync(bool includeDeleted = false);
        Task<Product?> GetByIdAsync(int id, bool includeDeleted = false);
        Task CreateAsync(ProductInputModel model);
        Task UpdateAsync(ProductInputModel model);
        Task DeleteAsync(int id, string deletedBy = "System");
        Task<bool> RestoreAsync(int id, string restoredBy = "System");
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}

