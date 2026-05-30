using System.ComponentModel;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class ReportEndpoints
{
    private const string FromDesc =
        "Inclusive start date (UTC), compared against Order.CreatedAt. " +
        "Format: yyyy-MM-dd, e.g. 2026-05-27.";

    private const string ToDesc =
        "Inclusive end date (UTC), compared against Order.CreatedAt. " +
        "Format: yyyy-MM-dd, e.g. 2026-05-28. The whole day is included " +
        "(treated as 23:59:59.999).";

    private const string TopDesc =
        "Maximum rows to return. Clamped to [1, 100]. Default 10.";

    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization("Admin");

        group.MapGet("/sales",
            async ([Description(FromDesc)] DateOnly? from,
                   [Description(ToDesc)]   DateOnly? to,
                   ReportService svc, CancellationToken ct) =>
                Results.Ok(await svc.SalesAsync(ToStart(from), ToEnd(to), ct)))
            .WithName("ReportSales")
            .Produces<IReadOnlyList<SalesPointDto>>();

        group.MapGet("/top-products",
            async ([Description(FromDesc)] DateOnly? from,
                   [Description(ToDesc)]   DateOnly? to,
                   [Description(TopDesc)]  int? top,
                   ReportService svc, CancellationToken ct) =>
                Results.Ok(await svc.TopProductsAsync(ToStart(from), ToEnd(to), Math.Clamp(top ?? 10, 1, 100), ct)))
            .WithName("ReportTopProducts")
            .Produces<IReadOnlyList<TopProductDto>>();

        group.MapGet("/top-customers",
            async ([Description(FromDesc)] DateOnly? from,
                   [Description(ToDesc)]   DateOnly? to,
                   [Description(TopDesc)]  int? top,
                   ReportService svc, CancellationToken ct) =>
                Results.Ok(await svc.TopCustomersAsync(ToStart(from), ToEnd(to), Math.Clamp(top ?? 10, 1, 100), ct)))
            .WithName("ReportTopCustomers")
            .Produces<IReadOnlyList<TopCustomerDto>>();

        group.MapGet("/low-stock",
            async (ReportService svc, CancellationToken ct) =>
                Results.Ok(await svc.LowStockAsync(ct)))
            .WithName("ReportLowStock")
            .Produces<IReadOnlyList<LowStockDto>>();

        return app;
    }

    private static DateTime? ToStart(DateOnly? d) =>
        d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : null;

    private static DateTime? ToEnd(DateOnly? d) =>
        d.HasValue ? d.Value.ToDateTime(new TimeOnly(23, 59, 59, 999), DateTimeKind.Utc) : null;
}
