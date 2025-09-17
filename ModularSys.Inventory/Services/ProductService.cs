using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Inventory.Interface;
using ModularSys.Inventory.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularSys.Inventory.Services
{
    public class ProductService : IProductService
    {
        private readonly InventoryDbContext _db;
        public ProductService(InventoryDbContext db) => _db = db;

        public async Task<IEnumerable<Product>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();
            return await query.FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task CreateAsync(ProductInputModel model)
        {
            ValidateProduct(model);

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

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductInputModel model)
        {
            ValidateProduct(model);

            var existing = await _db.Products.FindAsync(model.ProductId);
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

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string deletedBy)
        {
            var entity = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = deletedBy;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int id, string restoredBy)
        {
            var entity = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id && p.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = restoredBy;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Category>> GetCategoriesAsync() =>
            await _db.Categories.AsNoTracking().ToListAsync();

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

