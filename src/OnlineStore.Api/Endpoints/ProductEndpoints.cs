using Microsoft.AspNetCore.Mvc;
using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (
                [FromQuery] int? categoryId,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                ProductService svc,
                CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(categoryId, minPrice, maxPrice, ct)))
            .WithName("ListProducts")
            .Produces<List<ProductDto>>();

        group.MapGet("/{id:int}", async (int id, ProductService svc, CancellationToken ct) =>
            {
                var product = await svc.GetAsync(id, ct);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .WithName("GetProduct")
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/search", ([FromQuery] string q, [FromQuery] int take, ProductSearchIndex index) =>
            {
                var hits = index.Suggest(q ?? string.Empty, take == 0 ? 20 : Math.Clamp(take, 1, 50));
                return Results.Ok(hits.Select(h => new ProductSuggestion(h.Id, h.Name)).ToList());
            })
            .WithName("SearchProducts")
            .Produces<IReadOnlyList<ProductSuggestion>>();

        group.MapPost("/", async (CreateProductRequest req, ProductService svc, CancellationToken ct) =>
            {
                var result = await svc.CreateAsync(req, ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Created($"/api/products/{result.Value!.Id}", result.Value),
                    ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .RequireAuthorization("Admin")
            .WithName("CreateProduct")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateProductRequest req, ProductService svc, CancellationToken ct) =>
            {
                var result = await svc.UpdateAsync(id, req, ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Ok(result.Value),
                    ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                    ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .RequireAuthorization("Admin")
            .WithName("UpdateProduct")
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, ProductService svc, CancellationToken ct) =>
            {
                var deleted = await svc.DeleteAsync(id, ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization("Admin")
            .WithName("DeleteProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
