using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Services;

namespace ResourceManagement.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>Get a paged, filtered, sorted list of products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GET /products - page={Page}, pageSize={PageSize}, filter={Filter}", page, pageSize, filter);
        var result = await _productService.GetAllAsync(filter, sortBy, sortDescending, page, pageSize);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    /// <summary>Get a product by ID.</summary>
    [HttpGet("{id:int}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
            return NotFound(ApiResponse<ProductDto>.Fail($"Product with ID {id} not found."));
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>Create a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed.", validation.Errors.Select(e => e.ErrorMessage)));

        var product = await _productService.CreateAsync(dto);
        return CreatedAtRoute("GetProductById", new { id = product.Id }, ApiResponse<ProductDto>.Ok(product, "Product created successfully."));
    }

    /// <summary>Update an existing product.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed.", validation.Errors.Select(e => e.ErrorMessage)));

        var product = await _productService.UpdateAsync(id, dto);
        if (product is null)
            return NotFound(ApiResponse<ProductDto>.Fail($"Product with ID {id} not found."));

        return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully."));
    }

    /// <summary>Delete a product by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Product with ID {id} not found."));

        return Ok(ApiResponse<object>.Ok(null, "Product deleted successfully."));
    }
}
