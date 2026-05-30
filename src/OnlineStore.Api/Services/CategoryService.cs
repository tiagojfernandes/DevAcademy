using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public sealed class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest req, CancellationToken ct = default)
    {
        if (await _repo.NameExistsAsync(req.Name, ct))
            return Result<CategoryDto>.Conflict($"Category '{req.Name}' already exists.");

        var created = await _repo.CreateAsync(new Category { Name = req.Name }, ct);
        return Result<CategoryDto>.Ok(new CategoryDto(created.Id, created.Name));
    }
}

