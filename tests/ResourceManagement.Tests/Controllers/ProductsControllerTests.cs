using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceManagement.API.Controllers.V1;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Mappings;
using ResourceManagement.Application.Services;
using ResourceManagement.Application.Validators;
using Xunit;

namespace ResourceManagement.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        var logger = new Mock<ILogger<ProductsController>>();

        _controller = new ProductsController(
            _serviceMock.Object,
            new CreateProductValidator(),
            new UpdateProductValidator(),
            logger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResult()
    {
        var paged = new PagedResult<ProductDto>
        {
            Data = new[] { new ProductDto { Id = 1, ProductName = "P1" } },
            TotalCount = 1, Page = 1, PageSize = 10
        };
        _serviceMock.Setup(s => s.GetAllAsync(null, null, false, 1, 10)).ReturnsAsync(paged);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PagedResult<ProductDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data!.TotalCount);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new ProductDto { Id = 1, ProductName = "P1" });

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ProductDto?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreated()
    {
        var dto = new CreateProductDto { ProductName = "NewProd", CreatedBy = "admin" };
        var created = new ProductDto { Id = 5, ProductName = "NewProd" };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidDto_ReturnsBadRequest()
    {
        var dto = new CreateProductDto { ProductName = "", CreatedBy = "" };

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenFound_ReturnsOk()
    {
        var dto = new UpdateProductDto { ProductName = "Updated", ModifiedBy = "admin" };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto))
            .ReturnsAsync(new ProductDto { Id = 1, ProductName = "Updated" });

        var result = await _controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(ok.Value);
        Assert.True(response.Success);
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
