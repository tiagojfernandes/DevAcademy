using Microsoft.Extensions.Caching.Memory;
using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Events;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public sealed class ProductService
{
    private const string ListCacheKeyPrefix = "products:list:";
    private const string ByIdCacheKeyPrefix = "products:id:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _categories;
    private readonly IMemoryCache _cache;
    private readonly IEventBus _bus;

    public ProductService(IProductRepository repo, ICategoryRepository categories, IMemoryCache cache, IEventBus bus)
    {
        _repo       = repo;
        _categories = categories;
        _cache      = cache;
        _bus        = bus;
    }

    public async Task<ProductDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var key = ByIdCacheKeyPrefix + id;
        if (_cache.TryGetValue(key, out ProductDto? cached) && cached is not null)
            return cached;

        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null) return null;

        var dto = Map(product);
        _cache.Set(key, dto, CacheTtl);
        return dto;
    }

    public async Task<List<ProductDto>> ListAsync(
        int? categoryId, decimal? minPrice, decimal? maxPrice,
        CancellationToken ct = default)
    {
        var key = $"{ListCacheKeyPrefix}{categoryId}:{minPrice}:{maxPrice}";
        if (_cache.TryGetValue(key, out List<ProductDto>? cached) && cached is not null)
            return cached;

        var items = await _repo.ListAsync(categoryId, minPrice, maxPrice, ct);
        var result = items.Select(Map).ToList();

        _cache.Set(key, result, CacheTtl);
        return result;
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest req, CancellationToken ct = default)
    {
        if (!await _categories.ExistsAsync(req.CategoryId, ct))
            return Result<ProductDto>.Invalid($"Category {req.CategoryId} not found.");

        if (await _repo.SkuExistsAsync(req.SKU, null, ct))
            return Result<ProductDto>.Conflict($"SKU '{req.SKU}' already exists.");

        var entity = new Product
        {
            Name          = req.Name,
            Description   = req.Description,
            Price         = req.Price,
            SKU           = req.SKU,
            StockQuantity = req.StockQuantity,
            CategoryId    = req.CategoryId,
            LastModified  = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(entity, ct);
        _bus.Publish(new ProductCreatedEvent(created.Id, created.Name, DateTimeOffset.UtcNow));
        var withCategory = await _repo.GetByIdAsync(created.Id, ct);
        return Result<ProductDto>.Ok(Map(withCategory!));
    }

    public async Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductRequest req, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return Result<ProductDto>.NotFound($"Product {id} not found.");

        if (!await _categories.ExistsAsync(req.CategoryId, ct))
            return Result<ProductDto>.Invalid($"Category {req.CategoryId} not found.");

        if (!string.Equals(existing.SKU, req.SKU, StringComparison.Ordinal)
            && await _repo.SkuExistsAsync(req.SKU, id, ct))
            return Result<ProductDto>.Conflict($"SKU '{req.SKU}' already exists.");

        existing.Name          = req.Name;
        existing.Description   = req.Description;
        existing.Price         = req.Price;
        existing.SKU           = req.SKU;
        existing.StockQuantity = req.StockQuantity;
        existing.CategoryId    = req.CategoryId;

        var updated = await _repo.UpdateAsync(existing, ct);
        _cache.Remove(ByIdCacheKeyPrefix + id);
        _bus.Publish(new ProductUpdatedEvent(updated.Id, updated.Name, DateTimeOffset.UtcNow));

        var withCategory = await _repo.GetByIdAsync(updated.Id, ct);
        return Result<ProductDto>.Ok(Map(withCategory!));
    }

    // Returns false when the product does not exist.
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        await _repo.SoftDeleteAsync(id, ct);
        _cache.Remove(ByIdCacheKeyPrefix + id);
        _bus.Publish(new ProductDeletedEvent(existing.Id, DateTimeOffset.UtcNow));
        return true;
    }

    private static ProductDto Map(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.SKU, p.StockQuantity,
        p.CategoryId, p.Category?.Name ?? string.Empty);
}

