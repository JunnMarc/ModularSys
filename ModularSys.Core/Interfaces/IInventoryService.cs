using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Core.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Product>> GetAllItemsAsync();
        Task<Product?> GetItemByIdAsync(int id);
        Task<Product> AddItemAsync(Product item);
        Task<Product> UpdateItemAsync(Product item);
        Task<bool> DeleteItemAsync(int id);
    }
}
