namespace OnlineStore.Api.Entities;

// Maps to the ShoppingCartProducts table
public class CartItem
{
    public int ShoppingCartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public ShoppingCart? Cart { get; set; }
    public Product? Product { get; set; }
}
