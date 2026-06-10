using AutoMapper;
using Moq;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Mappings;
using ResourceManagement.Application.Services;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using Xunit;

namespace ResourceManagement.Tests.Services;

public class ItemServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IItemRepository> _itemRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _itemRepoMock = new Mock<IItemRepository>();
        _productRepoMock = new Mock<IProductRepository>();

        _uowMock.Setup(u => u.Items).Returns(_itemRepoMock.Object);
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<ItemMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new ItemService(_uowMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByProductIdAsync_ReturnsItems()
    {
        var items = new List<Item>
        {
            new Item { Id = 1, ProductId = 1, Quantity = 10 },
            new Item { Id = 2, ProductId = 1, Quantity = 20 }
        };
        _itemRepoMock.Setup(r => r.GetByProductIdAsync(1)).ReturnsAsync(items);

        var result = await _service.GetByProductIdAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsDto()
    {
        var item = new Item { Id = 1, ProductId = 1, Quantity = 50 };
        _itemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(50, result!.Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _itemRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Item?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenProductExists_CreatesItem()
    {
        var product = new Product { Id = 1, ProductName = "P", CreatedBy = "test", CreatedOn = DateTime.UtcNow, Items = new List<Item>() };
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _itemRepoMock.Setup(r => r.AddAsync(It.IsAny<Item>())).ReturnsAsync((Item i) => i);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new CreateItemDto { ProductId = 1, Quantity = 100 };
        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(100, result!.Quantity);
    }

    [Fact]
    public async Task CreateAsync_WhenProductNotFound_ReturnsNull()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.CreateAsync(new CreateItemDto { ProductId = 99, Quantity = 1 });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesAndReturns()
    {
        var item = new Item { Id = 1, ProductId = 1, Quantity = 10 };
        _itemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _itemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Item>())).ReturnsAsync((Item i) => i);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.UpdateAsync(1, new UpdateItemDto { Quantity = 999 });

        Assert.NotNull(result);
        Assert.Equal(999, result!.Quantity);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ReturnsTrue()
    {
        var item = new Item { Id = 1, ProductId = 1, Quantity = 5 };
        _itemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _itemRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Item?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
    }
}
