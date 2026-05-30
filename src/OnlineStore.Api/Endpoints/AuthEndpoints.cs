using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Services;

namespace OnlineStore.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest req, AuthService svc, CancellationToken ct) =>
        {
            var result = await svc.RegisterAsync(req.Username, req.Password, ct);
            return result.Status switch
            {
                ResultStatus.Ok       => Results.Created($"/api/users/{result.Value!.UserId}", result.Value),
                ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
                _                     => Results.Problem()
            };
        })
        .WithName("Register")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest req, AuthService svc, CancellationToken ct) =>
        {
            var auth = await svc.LoginAsync(req.Username, req.Password, ct);
            return auth is null
                ? Results.Unauthorized()
                : Results.Ok(auth);
        })
        .WithName("Login")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
