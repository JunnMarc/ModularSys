using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<InventoryDbContext> _contextFactory;

        public CategoryService(IDbContextFactory<InventoryDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var db = _contextFactory.CreateDbContext();
            return await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using var db = _contextFactory.CreateDbContext();
            return await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task CreateAsync(Category category)
        {
            using var db = _contextFactory.CreateDbContext();
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            using var db = _contextFactory.CreateDbContext();
            db.Categories.Update(category);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _contextFactory.CreateDbContext();
            var existing = await db.Categories.FindAsync(id);
            if (existing != null)
            {
                db.Categories.Remove(existing);
                await db.SaveChangesAsync();
            }
        }
    }
}
