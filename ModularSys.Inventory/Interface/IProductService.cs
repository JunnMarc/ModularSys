using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Models;

namespace ModularSys.Inventory.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(ProductInputModel model);
        Task UpdateAsync(ProductInputModel model);
        Task DeleteAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}

