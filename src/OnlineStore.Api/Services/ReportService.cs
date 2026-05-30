using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Data;
using OnlineStore.Api.DTOs;

namespace OnlineStore.Api.Services;

/// <summary>
/// Admin reporting queries. Uses keyless views where possible (dbo.vw_SalesDaily,
/// dbo.vw_LowStock) and direct LINQ aggregation for the date-filtered ones.
/// </summary>
public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SalesPointDto>> SalesAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = _db.SalesDaily.AsNoTracking().AsQueryable();
        if (from.HasValue) q = q.Where(r => r.SalesDate >= from.Value.Date);
        if (to.HasValue)   q = q.Where(r => r.SalesDate <= to.Value.Date);

        return await q
            .OrderBy(r => r.SalesDate)
            .Select(r => new SalesPointDto(r.SalesDate, r.OrderCount, r.Revenue, r.UnitsSold))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopProductDto>> TopProductsAsync(DateTime? from, DateTime? to, int top, CancellationToken ct)
    {
        var q = _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order!.Status != "Cancelled");

        if (from.HasValue) q = q.Where(i => i.Order!.CreatedAt >= from.Value);
        if (to.HasValue)   q = q.Where(i => i.Order!.CreatedAt <= to.Value);

        var rows = await q
            .GroupBy(i => new { i.ProductId, i.Product!.Name, i.Product.SKU })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                g.Key.SKU,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue   = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(d => d.Revenue)
            .Take(top)
            .ToListAsync(ct);

        return rows
            .Select(r => new TopProductDto(r.ProductId, r.Name, r.SKU, r.UnitsSold, r.Revenue))
            .ToList();
    }

    public async Task<IReadOnlyList<TopCustomerDto>> TopCustomersAsync(DateTime? from, DateTime? to, int top, CancellationToken ct)
    {
        var q = _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order!.Status != "Cancelled");

        if (from.HasValue) q = q.Where(i => i.Order!.CreatedAt >= from.Value);
        if (to.HasValue)   q = q.Where(i => i.Order!.CreatedAt <= to.Value);

        var rows = await q
            .GroupBy(i => new { i.Order!.UserId, i.Order.User!.Username })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.Username,
                OrderCount = g.Select(x => x.OrderId).Distinct().Count(),
                Revenue    = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(d => d.Revenue)
            .Take(top)
            .ToListAsync(ct);

        return rows
            .Select(r => new TopCustomerDto(r.UserId, r.Username, r.OrderCount, r.Revenue))
            .ToList();
    }

    public async Task<IReadOnlyList<LowStockDto>> LowStockAsync(CancellationToken ct) =>
        await _db.LowStock
            .AsNoTracking()
            .OrderBy(r => r.StockQuantity)
            .Select(r => new LowStockDto(r.Id, r.Name, r.SKU, r.StockQuantity, r.CategoryName))
            .ToListAsync(ct);
}
