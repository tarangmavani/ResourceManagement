using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetByProductIdAsync(int productId);
    Task<Item?> GetByIdAsync(int id);
    Task<Item> AddAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task DeleteAsync(Item item);
}
