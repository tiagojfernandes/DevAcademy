namespace OnlineStore.Api.Entities;

public static class OrderStatus
{
    public const string Pending   = "Pending";
    public const string Shipped   = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { Pending, Shipped, Delivered, Cancelled };
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = OrderStatus.Pending;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
