using OnlineStore.Api.Entities;

namespace OnlineStore.Api.Interfaces;

public interface ICartRepository
{
    Task<ShoppingCart> GetOrCreateAsync(int userId, CancellationToken ct = default);
    Task<ShoppingCart?> GetWithItemsAsync(int userId, CancellationToken ct = default);
    Task AddOrUpdateItemAsync(int cartId, int productId, int quantity, decimal unitPrice, CancellationToken ct = default);
    Task RemoveItemAsync(int cartId, int productId, CancellationToken ct = default);
    Task ClearAsync(int cartId, CancellationToken ct = default);
}
