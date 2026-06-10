using Microsoft.EntityFrameworkCore;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(p => p.ModifiedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Quantity).IsRequired();
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Items)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
