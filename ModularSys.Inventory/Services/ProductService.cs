using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using ModularSys.Inventory.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularSys.Inventory.Services
{
    public class ProductService : IProductService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public ProductService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public async Task<IEnumerable<Product>> GetAllAsync(bool includeDeleted = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            
            var query = db.Products.Include(p => p.Category).AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            
            var query = db.Products.Include(p => p.Category).AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task CreateAsync(ProductInputModel model)
        {
            ValidateProduct(model);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var entity = new Product
            {
                SKU = model.SKU ?? $"SKU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                Name = model.Name,
                Description = model.Description,
                Barcode = model.Barcode,
                CategoryId = model.CategoryId,
                UnitPrice = model.UnitPrice,
                CostPrice = model.CostPrice ?? model.UnitPrice * 0.7m, // Default cost price if not provided
                QuantityOnHand = model.QuantityOnHand,
                ReorderLevel = model.ReorderLevel,
                MinStockLevel = model.MinStockLevel,
                MaxStockLevel = model.MaxStockLevel,
                ExpiryDate = model.ExpiryDate,
                BatchNumber = model.BatchNumber,
                Supplier = model.Supplier,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            db.Products.Add(entity);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductInputModel model)
        {
            ValidateProduct(model);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var existing = await db.Products.FindAsync(model.ProductId);
            if (existing == null)
                throw new KeyNotFoundException("Product not found.");

            existing.SKU = model.SKU ?? existing.SKU;
            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Barcode = model.Barcode;
            existing.CategoryId = model.CategoryId;
            existing.UnitPrice = model.UnitPrice;
            existing.CostPrice = model.CostPrice ?? existing.CostPrice;
            existing.QuantityOnHand = model.QuantityOnHand;
            existing.ReorderLevel = model.ReorderLevel;
            existing.MinStockLevel = model.MinStockLevel;
            existing.MaxStockLevel = model.MaxStockLevel;
            existing.ExpiryDate = model.ExpiryDate;
            existing.BatchNumber = model.BatchNumber;
            existing.Supplier = model.Supplier;

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var entity = await db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = deletedBy;
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var entity = await db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id && p.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = restoredBy;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            return await db.Categories.AsNoTracking().ToListAsync();
        }

        private void ValidateProduct(ProductInputModel model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, context, results, true))
            {
                var errors = string.Join("\n", results.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}

