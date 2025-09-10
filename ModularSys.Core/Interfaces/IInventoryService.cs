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
        Task<IEnumerable<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem?> GetItemByIdAsync(int id);
        Task<InventoryItem> AddItemAsync(InventoryItem item);
        Task<InventoryItem> UpdateItemAsync(InventoryItem item);
        Task<bool> DeleteItemAsync(int id);
    }
}
