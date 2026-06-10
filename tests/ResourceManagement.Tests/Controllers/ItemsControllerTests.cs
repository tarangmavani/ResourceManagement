using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceManagement.API.Controllers.V1;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Services;
using ResourceManagement.Application.Validators;
using Xunit;

namespace ResourceManagement.Tests.Controllers;

public class ItemsControllerTests
{
    private readonly Mock<IItemService> _serviceMock;
    private readonly ItemsController _controller;

    public ItemsControllerTests()
    {
        _serviceMock = new Mock<IItemService>();
        var logger = new Mock<ILogger<ItemsController>>();

        _controller = new ItemsController(
            _serviceMock.Object,
            new CreateItemValidator(),
            new UpdateItemValidator(),
            logger.Object);
    }

    [Fact]
    public async Task GetByProduct_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByProductIdAsync(1))
            .ReturnsAsync(new[] { new ItemDto { Id = 1, ProductId = 1, Quantity = 10 } });

        var result = await _controller.GetByProduct(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ItemDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new ItemDto { Id = 1, ProductId = 1, Quantity = 5 });

        var result = await _controller.GetById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ItemDto?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreated()
    {
        var dto = new CreateItemDto { ProductId = 1, Quantity = 50 };
        _serviceMock.Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(new ItemDto { Id = 1, ProductId = 1, Quantity = 50 });

        var result = await _controller.Create(dto);

        Assert.IsType<CreatedAtRouteResult>(result);
    }

    [Fact]
    public async Task Create_InvalidDto_ReturnsBadRequest()
    {
        var dto = new CreateItemDto { ProductId = 0, Quantity = -1 };

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenProductNotFound_ReturnsNotFound()
    {
        var dto = new CreateItemDto { ProductId = 99, Quantity = 10 };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync((ItemDto?)null);

        var result = await _controller.Create(dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenFound_ReturnsOk()
    {
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateItemDto>()))
            .ReturnsAsync(new ItemDto { Id = 1, ProductId = 1, Quantity = 100 });

        var result = await _controller.Update(1, new UpdateItemDto { Quantity = 100 });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenFound_ReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _controller.Delete(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
