using AutoMapper;
using Moq;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Mappings;
using ResourceManagement.Application.Services;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using Xunit;

namespace ResourceManagement.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProductRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IProductRepository>();

        _uowMock.Setup(u => u.Products).Returns(_repoMock.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<ItemMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new ProductService(_uowMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaged()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, ProductName = "Alpha", CreatedBy = "test", CreatedOn = DateTime.UtcNow, Items = new List<Item>() },
            new Product { Id = 2, ProductName = "Beta",  CreatedBy = "test", CreatedOn = DateTime.UtcNow, Items = new List<Item>() }
        };
        _repoMock.Setup(r => r.GetAllAsync(null, null, false, 1, 10)).ReturnsAsync(products);
        _repoMock.Setup(r => r.CountAsync(null)).ReturnsAsync(2);

        // Act
        var result = await _service.GetAllAsync(null, null, false, 1, 10);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var product = new Product { Id = 1, ProductName = "Alpha", CreatedBy = "test", CreatedOn = DateTime.UtcNow, Items = new List<Item>() };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Alpha", result!.ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_SavesAndReturnsDto()
    {
        var dto = new CreateProductDto { ProductName = "NewProduct", CreatedBy = "admin" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        Assert.Equal("NewProduct", result.ProductName);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesAndReturnsDto()
    {
        var existing = new Product { Id = 1, ProductName = "Old", CreatedBy = "admin", CreatedOn = DateTime.UtcNow, Items = new List<Item>() };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new UpdateProductDto { ProductName = "Updated", ModifiedBy = "admin" };
        var result = await _service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result!.ProductName);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(99, new UpdateProductDto { ProductName = "X", ModifiedBy = "Y" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ReturnsTrue()
    {
        var product = new Product { Id = 1, ProductName = "Del", CreatedBy = "admin", CreatedOn = DateTime.UtcNow, Items = new List<Item>() };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
    }
}
