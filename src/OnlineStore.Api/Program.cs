using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Api.Common;
using OnlineStore.Api.Data;
using OnlineStore.Api.Endpoints;
using OnlineStore.Api.Events;
using OnlineStore.Api.Interfaces;
using OnlineStore.Api.Repositories;
using OnlineStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration ----------
var jwtSecret   = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Missing 'Jwt:Secret' configuration value.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// ---------- Persistence ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ---------- Telemetry (Application Insights) ----------
// Picks up APPLICATIONINSIGHTS_CONNECTION_STRING from env vars automatically.
builder.Services.AddApplicationInsightsTelemetry();

// ---------- Caching ----------
builder.Services.AddMemoryCache();

// ---------- DI ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasherService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddSingleton<ProductSearchIndex>();

// ---------- AuthN / AuthZ ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
    options.AddPolicy("User",  p => p.RequireRole("User", "Admin"));
});

// ---------- ProblemDetails + global exception handler ----------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------- OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "OnlineStore API",
        Version     = "v1",
        Description = "Order Management System API with JWT authentication."
    });

    var bearer = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        Description  = "Paste the JWT token below (no 'Bearer ' prefix).",
        Reference    = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id   = "Bearer",
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", bearer);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [bearer] = Array.Empty<string>()
    });
});

var app = builder.Build();

// ---------- Event subscriptions (observer wiring) ----------
var eventBus = app.Services.GetRequiredService<IEventBus>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var telemetry = app.Services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>();

// Two business events worth tracking in App Insights.
eventBus.Subscribe<UserRegisteredEvent>(e =>
{
    logger.LogInformation("User {Id} registered at {At}", e.UserId, e.OccurredAt);
    telemetry.TrackEvent("UserRegistered", new Dictionary<string, string> { ["userId"] = e.UserId.ToString() });
});

eventBus.Subscribe<OrderPlacedEvent>(e =>
{
    logger.LogInformation("Order {OrderId} placed by user {UserId} total={Total}", e.OrderId, e.UserId, e.TotalPrice);
    telemetry.TrackEvent("OrderPlaced",
        new Dictionary<string, string>
        {
            ["orderId"] = e.OrderId.ToString(),
            ["userId"]  = e.UserId.ToString()
        },
        new Dictionary<string, double> { ["totalPrice"] = (double)e.TotalPrice });
});

// Keep the in-memory autocomplete index in sync with product changes.
var searchIndex = app.Services.GetRequiredService<ProductSearchIndex>();
eventBus.Subscribe<ProductCreatedEvent>(e => searchIndex.Insert(e.ProductId, e.Name));
eventBus.Subscribe<ProductUpdatedEvent>(e => { searchIndex.Remove(e.ProductId); searchIndex.Insert(e.ProductId, e.Name); });
eventBus.Subscribe<ProductDeletedEvent>(e => searchIndex.Remove(e.ProductId));

// Initial population from the DB.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seed = await db.Products.AsNoTracking()
        .Where(p => !p.IsDeleted)
        .Select(p => new { p.Id, p.Name })
        .ToListAsync();
    searchIndex.Rebuild(seed.Select(x => (x.Id, x.Name)));
    logger.LogInformation("ProductSearchIndex bootstrapped with {Count} products", seed.Count);
}

// ---------- HTTP pipeline ----------
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();

public partial class Program { }
