using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Data;

// IdentityDbContext<ApplicationUser> (instead of plain DbContext) is what brings
// in ASP.NET Core Identity's own tables — AspNetUsers, AspNetRoles,
// AspNetUserRoles, AspNetUserClaims, etc. — alongside our own Products/Orders
// tables below. A single generic parameter is enough here: IdentityDbContext<TUser>
// already uses IdentityRole/string keys internally, which is all this template needs.
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required by IdentityDbContext to configure its own tables. Forgetting
        // this call is a common mistake — the app builds fine but migrations
        // won't include the Identity schema at all.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
