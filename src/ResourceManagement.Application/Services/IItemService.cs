using ResourceManagement.Application.DTOs;

namespace ResourceManagement.Application.Services;

public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetByProductIdAsync(int productId);
    Task<ItemDto?> GetByIdAsync(int id);
    Task<ItemDto?> CreateAsync(CreateItemDto dto);
    Task<ItemDto?> UpdateAsync(int id, UpdateItemDto dto);
    Task<bool> DeleteAsync(int id);
}
