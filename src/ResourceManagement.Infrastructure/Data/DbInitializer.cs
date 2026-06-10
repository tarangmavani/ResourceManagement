using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Applying EF Core migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            if (!await context.Products.AnyAsync())
            {
                logger.LogInformation("Seeding database with sample data...");

                var products = new List<Product>
                {
                    new Product
                    {
                        ProductName = "Widget Alpha",
                        CreatedBy = "system",
                        CreatedOn = DateTime.UtcNow,
                        Items = new List<Item>
                        {
                            new Item { Quantity = 100 },
                            new Item { Quantity = 250 }
                        }
                    },
                    new Product
                    {
                        ProductName = "Gadget Beta",
                        CreatedBy = "system",
                        CreatedOn = DateTime.UtcNow,
                        Items = new List<Item>
                        {
                            new Item { Quantity = 50 }
                        }
                    },
                    new Product
                    {
                        ProductName = "Component Gamma",
                        CreatedBy = "system",
                        CreatedOn = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Database seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
