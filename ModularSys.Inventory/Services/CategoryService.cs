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

        public async Task<IEnumerable<Category>> GetAllAsync(bool includeDeleted = false)
        {
            using var db = _contextFactory.CreateDbContext();
            var query = db.Categories.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var db = _contextFactory.CreateDbContext();
            var query = db.Categories.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
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

        public async Task DeleteAsync(int id, string deletedBy)
        {
            using var db = _contextFactory.CreateDbContext();
            var existing = await db.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == id);
            if (existing != null)
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = deletedBy;
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy)
        {
            using var db = _contextFactory.CreateDbContext();
            var existing = await db.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == id && c.IsDeleted);
            if (existing == null) return false;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedBy = null;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = restoredBy;
            await db.SaveChangesAsync();
            return true;
        }
    }
}
