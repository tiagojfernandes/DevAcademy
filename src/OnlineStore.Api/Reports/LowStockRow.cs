namespace OnlineStore.Api.Reports;

/// <summary>Keyless projection of dbo.vw_LowStock.</summary>
public class LowStockRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
