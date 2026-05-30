using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Data;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken ct = default) =>
        await _db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> ListAllAsync(CancellationToken ct = default) =>
        await _db.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .ToListAsync(ct);
}
