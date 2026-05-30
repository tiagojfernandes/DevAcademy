namespace OnlineStore.Api.Endpoints;

public static class EndpointMapping
{
    /// <summary>Single call from Program.cs that registers every feature's endpoints.</summary>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapCategoryEndpoints();
        app.MapProductEndpoints();
        app.MapCartEndpoints();
        app.MapOrderEndpoints();
        app.MapReportEndpoints();
        return app;
    }
}
