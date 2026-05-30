using OnlineStore.Api.Entities;

namespace OnlineStore.Api.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListAsync(
        int? categoryId, decimal? minPrice, decimal? maxPrice,
        CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken ct = default);
}
