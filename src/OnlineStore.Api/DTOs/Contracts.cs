namespace OnlineStore.Api.DTOs;

// ---- Auth ----
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    public AuthResponse() { }
    public AuthResponse(int userId, string username, string token, IReadOnlyList<string> roles)
    {
        UserId = userId;
        Username = username;
        Token = token;
        Roles = roles;
    }
}

// ---- Categories ----
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public CategoryDto() { }
    public CategoryDto(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

// ---- Products ----
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public ProductDto() { }
    public ProductDto(int id, string name, string? description, decimal price, string sku, int stockQuantity, int categoryId, string categoryName)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        SKU = sku;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        CategoryName = categoryName;
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
}

// ---- Search suggestions ----
public class ProductSuggestion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ProductSuggestion() { }
    public ProductSuggestion(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

// ---- Cart ----
public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public CartItemDto() { }
    public CartItemDto(int productId, string productName, int quantity, decimal unitPrice, decimal lineTotal)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}

public class CartDto
{
    public int Id { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public decimal Total { get; set; }

    public CartDto() { }
    public CartDto(int id, IReadOnlyList<CartItemDto> items, decimal total)
    {
        Id = id;
        Items = items;
        Total = total;
    }
}

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

// ---- Orders ----
public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public OrderItemDto() { }
    public OrderItemDto(int productId, string productName, decimal unitPrice, int quantity, decimal lineTotal)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        LineTotal = lineTotal;
    }
}

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();

    public OrderDto() { }
    public OrderDto(int id, int userId, string status, decimal totalPrice, DateTime createdAt, IReadOnlyList<OrderItemDto> items)
    {
        Id = id;
        UserId = userId;
        Status = status;
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
        Items = items;
    }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

// ---- Reports ----
public class SalesPointDto
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public int UnitsSold { get; set; }

    public SalesPointDto() { }
    public SalesPointDto(DateTime date, int orderCount, decimal revenue, int unitsSold)
    {
        Date = date;
        OrderCount = orderCount;
        Revenue = revenue;
        UnitsSold = unitsSold;
    }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }

    public TopProductDto() { }
    public TopProductDto(int productId, string name, string sku, int unitsSold, decimal revenue)
    {
        ProductId = productId;
        Name = name;
        SKU = sku;
        UnitsSold = unitsSold;
        Revenue = revenue;
    }
}

public class TopCustomerDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }

    public TopCustomerDto() { }
    public TopCustomerDto(int userId, string username, int orderCount, decimal revenue)
    {
        UserId = userId;
        Username = username;
        OrderCount = orderCount;
        Revenue = revenue;
    }
}

public class LowStockDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public LowStockDto() { }
    public LowStockDto(int id, string name, string sku, int stockQuantity, string categoryName)
    {
        Id = id;
        Name = name;
        SKU = sku;
        StockQuantity = stockQuantity;
        CategoryName = categoryName;
    }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public int UnitsTotal { get; set; }
}

public class ProductCatalogDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

