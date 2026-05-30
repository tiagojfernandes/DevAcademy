using System.Security.Claims;
using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("/", async (ClaimsPrincipal user, OrderService svc, CancellationToken ct) =>
            {
                var result = await svc.PlaceOrderAsync(GetUserId(user), ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Created($"/api/orders/{result.Value!.Id}", result.Value),
                    ResultStatus.Invalid  => Results.BadRequest(new { error = result.Error }),
                    ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                    ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .RequireAuthorization("User")
            .WithName("PlaceOrder")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/me", async (ClaimsPrincipal user, OrderService svc, CancellationToken ct) =>
                Results.Ok(await svc.ListMineAsync(GetUserId(user), ct)))
            .RequireAuthorization("User")
            .WithName("ListMyOrders")
            .Produces<IReadOnlyList<OrderDto>>();

        group.MapGet("/", async (OrderService svc, CancellationToken ct) =>
                Results.Ok(await svc.ListAllAsync(ct)))
            .RequireAuthorization("Admin")
            .WithName("ListAllOrders")
            .Produces<IReadOnlyList<OrderDto>>();

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, OrderService svc, CancellationToken ct) =>
            {
                var order = await svc.GetAsync(id, GetUserId(user), user.IsInRole("Admin"), ct);
                return order is null ? Results.NotFound() : Results.Ok(order);
            })
            .RequireAuthorization("User")
            .WithName("GetOrder")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/status", async (int id, UpdateOrderStatusRequest req, OrderService svc, CancellationToken ct) =>
            {
                var result = await svc.UpdateStatusAsync(id, req.Status, ct);
                return result.Status switch
                {
                    ResultStatus.Ok       => Results.Ok(result.Value),
                    ResultStatus.Invalid  => Results.BadRequest(new { error = result.Error }),
                    ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                    _                     => Results.Problem()
                };
            })
            .RequireAuthorization("Admin")
            .WithName("UpdateOrderStatus")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
