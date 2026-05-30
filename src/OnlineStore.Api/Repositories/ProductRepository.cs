using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Data;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    private IQueryable<Product> Active => _db.Products.AsNoTracking().Where(p => !p.IsDeleted);

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Active.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> ListAsync(
        int? categoryId, decimal? minPrice, decimal? maxPrice,
        CancellationToken ct = default)
    {
        var q = Active.Include(p => p.Category).AsQueryable();

        if (categoryId is not null) q = q.Where(p => p.CategoryId == categoryId);
        if (minPrice   is not null) q = q.Where(p => p.Price >= minPrice);
        if (maxPrice   is not null) q = q.Where(p => p.Price <= maxPrice);

        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.LastModified = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null || entity.IsDeleted) return;
        entity.IsDeleted = true;
        entity.LastModified = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.SKU == sku && (excludeId == null || p.Id != excludeId), ct);
}
