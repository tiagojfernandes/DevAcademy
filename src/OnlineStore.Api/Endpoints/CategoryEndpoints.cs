using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (CategoryService svc, CancellationToken ct) =>
                Results.Ok(await svc.GetAllAsync(ct)))
            .WithName("ListCategories")
            .Produces<IReadOnlyList<CategoryDto>>();

        group.MapPost("/", async (CreateCategoryRequest req, CategoryService svc, CancellationToken ct) =>
            {
                var result = await svc.CreateAsync(req, ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Created($"/api/categories/{result.Value!.Id}", result.Value),
                    ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .RequireAuthorization("Admin")
            .WithName("CreateCategory")
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
