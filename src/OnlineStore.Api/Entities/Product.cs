namespace OnlineStore.Api.Entities;

public class Product
{
    public int      Id            { get; set; }
    public string   Name          { get; set; } = string.Empty;
    public string?  Description   { get; set; }
    public decimal  Price         { get; set; }
    public string   SKU           { get; set; } = string.Empty;
    public int      StockQuantity { get; set; }
    public int      CategoryId    { get; set; }
    public bool     IsDeleted     { get; set; }
    public DateTime LastModified  { get; set; }

    public Category Category { get; set; } = null!;
}
