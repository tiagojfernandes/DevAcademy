namespace OnlineStore.Api.Reports;

/// <summary>Keyless projection of dbo.vw_SalesDaily.</summary>
public class SalesDailyRow
{
    public DateTime SalesDate { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public int UnitsSold { get; set; }
}
