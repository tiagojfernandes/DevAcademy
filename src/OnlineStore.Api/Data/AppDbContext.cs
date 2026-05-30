using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Reports;

namespace OnlineStore.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // SQL views (read-only, backed by dbo.vw_*)
    public DbSet<SalesDailyRow> SalesDaily => Set<SalesDailyRow>();
    public DbSet<LowStockRow> LowStock => Set<LowStockRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The SQL schema (infra/sql/schema.sql) is the source of truth for column types,
        // lengths, defaults and indexes.

        modelBuilder.Entity<Category>().ToTable("Categories");

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");

        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<ShoppingCart>(e =>
        {
            e.ToTable("ShoppingCart");
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
            e.HasMany(c => c.Items).WithOne(i => i.Cart!).HasForeignKey(i => i.ShoppingCartId);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("ShoppingCartProducts");
            e.HasKey(i => new { i.ShoppingCartId, i.ProductId });
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId);
            e.HasMany(o => o.Items).WithOne(i => i.Order!).HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
            // LineTotal is computed in SQL (UnitPrice * Quantity); EF must not try to write it.
            e.Property(i => i.LineTotal).HasComputedColumnSql("[UnitPrice] * [Quantity]", stored: true);
        });

        // Views (read-only)
        modelBuilder.Entity<SalesDailyRow>(e => { e.HasNoKey(); e.ToView("vw_SalesDaily"); });
        modelBuilder.Entity<LowStockRow>(e =>  { e.HasNoKey(); e.ToView("vw_LowStock"); });
    }
}
