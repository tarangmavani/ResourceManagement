using Microsoft.EntityFrameworkCore;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Infrastructure.Data;
using ResourceManagement.Infrastructure.Repositories;
using Xunit;

namespace ResourceManagement.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductRepository _repo;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repo = new ProductRepository(_context);
    }

    private Product MakeProduct(string name) => new Product
    {
        ProductName = name,
        CreatedBy = "test",
        CreatedOn = DateTime.UtcNow,
        Items = new List<Item>()
    };

    [Fact]
    public async Task AddAsync_ThenCount_ReturnsOne()
    {
        var p = MakeProduct("Prod1");
        await _repo.AddAsync(p);
        await _context.SaveChangesAsync();

        var count = await _repo.CountAsync(null);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsProduct()
    {
        var p = MakeProduct("Test");
        await _repo.AddAsync(p);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(p.Id);
        Assert.NotNull(found);
        Assert.Equal("Test", found!.ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(9999);
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_FiltersCorrectly()
    {
        await _repo.AddAsync(MakeProduct("Alpha"));
        await _repo.AddAsync(MakeProduct("Beta"));
        await _context.SaveChangesAsync();

        var results = await _repo.GetAllAsync("Alpha", null, false, 1, 10);
        Assert.Single(results);
        Assert.Equal("Alpha", results.First().ProductName);
    }

    [Fact]
    public async Task GetAllAsync_Paging_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 5; i++)
            await _repo.AddAsync(MakeProduct($"Product{i}"));
        await _context.SaveChangesAsync();

        var page2 = await _repo.GetAllAsync(null, null, false, 2, 2);
        Assert.Equal(2, page2.Count());
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var p = MakeProduct("ToDelete");
        await _repo.AddAsync(p);
        await _context.SaveChangesAsync();

        await _repo.DeleteAsync(p);
        await _context.SaveChangesAsync();

        var count = await _repo.CountAsync(null);
        Assert.Equal(0, count);
    }

    public void Dispose() => _context.Dispose();
}
