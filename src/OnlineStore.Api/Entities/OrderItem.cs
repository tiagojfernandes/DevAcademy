namespace OnlineStore.Api.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    // Computed in SQL: UnitPrice * Quantity.
    public decimal LineTotal { get; private set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
