using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart")
            .RequireAuthorization("User");

        group.MapGet("/", async (ClaimsPrincipal user, CartService svc, CancellationToken ct) =>
                Results.Ok(await svc.GetAsync(GetUserId(user), ct)))
            .WithName("GetCart")
            .Produces<CartDto>();

        group.MapPost("/items", async (AddCartItemRequest req, ClaimsPrincipal user, CartService svc, CancellationToken ct) =>
            {
                var result = await svc.AddItemAsync(GetUserId(user), req, ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Ok(result.Value),
                    ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                    ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .WithName("AddCartItem")
            .Produces<CartDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/items/{productId:int}", async (int productId, ClaimsPrincipal user, CartService svc, CancellationToken ct) =>
                Results.Ok(await svc.RemoveItemAsync(GetUserId(user), productId, ct)))
            .WithName("RemoveCartItem")
            .Produces<CartDto>();

        return app;
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub")
                 ?? throw new InvalidOperationException("Missing user id claim on token.");
        return int.Parse(value);
    }
}
