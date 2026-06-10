using ResourceManagement.Application.DTOs;

namespace ResourceManagement.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(string? filter, string? sortBy, bool sortDescending, int page, int pageSize);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}
