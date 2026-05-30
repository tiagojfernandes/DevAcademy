using OnlineStore.Api.Entities;

namespace OnlineStore.Api.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListAllAsync(CancellationToken ct = default);
}
