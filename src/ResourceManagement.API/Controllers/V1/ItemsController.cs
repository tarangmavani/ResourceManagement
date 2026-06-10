using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Application.Services;

namespace ResourceManagement.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly IValidator<CreateItemDto> _createValidator;
    private readonly IValidator<UpdateItemDto> _updateValidator;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(
        IItemService itemService,
        IValidator<CreateItemDto> createValidator,
        IValidator<UpdateItemDto> updateValidator,
        ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>Get all items for a product.</summary>
    [HttpGet("by-product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ItemDto>>), 200)]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        _logger.LogInformation("GET /items/by-product/{ProductId}", productId);
        var items = await _itemService.GetByProductIdAsync(productId);
        return Ok(ApiResponse<IEnumerable<ItemDto>>.Ok(items));
    }

    /// <summary>Get an item by ID.</summary>
    [HttpGet("{id:int}", Name = "GetItemById")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _itemService.GetByIdAsync(id);
        if (item is null)
            return NotFound(ApiResponse<ItemDto>.Fail($"Item with ID {id} not found."));
        return Ok(ApiResponse<ItemDto>.Ok(item));
    }

    /// <summary>Create a new item.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ItemDto>.Fail("Validation failed.", validation.Errors.Select(e => e.ErrorMessage)));

        var item = await _itemService.CreateAsync(dto);
        if (item is null)
            return NotFound(ApiResponse<ItemDto>.Fail($"Product with ID {dto.ProductId} not found."));

        return CreatedAtRoute("GetItemById", new { id = item.Id }, ApiResponse<ItemDto>.Ok(item, "Item created successfully."));
    }

    /// <summary>Update an item.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ItemDto>.Fail("Validation failed.", validation.Errors.Select(e => e.ErrorMessage)));

        var item = await _itemService.UpdateAsync(id, dto);
        if (item is null)
            return NotFound(ApiResponse<ItemDto>.Fail($"Item with ID {id} not found."));

        return Ok(ApiResponse<ItemDto>.Ok(item, "Item updated successfully."));
    }

    /// <summary>Delete an item.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _itemService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Item with ID {id} not found."));

        return Ok(ApiResponse<object>.Ok(null, "Item deleted successfully."));
    }
}
