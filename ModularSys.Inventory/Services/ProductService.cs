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

        public async Task<IEnumerable<Product>> GetAllAsync() =>
            await _db.Products.Include(p => p.Category).AsNoTracking().ToListAsync();

        public async Task<Product?> GetByIdAsync(int id) =>
            await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);

        public async Task CreateAsync(ProductInputModel model)
        {
            ValidateProduct(model);

            var entity = new Product
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                UnitPrice = model.UnitPrice,
                QuantityOnHand = model.QuantityOnHand,
                ReorderLevel = model.ReorderLevel
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

            existing.Name = model.Name;
            existing.CategoryId = model.CategoryId;
            existing.UnitPrice = model.UnitPrice;
            existing.QuantityOnHand = model.QuantityOnHand;
            existing.ReorderLevel = model.ReorderLevel;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity != null)
            {
                _db.Products.Remove(entity);
                await _db.SaveChangesAsync();
            }
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

