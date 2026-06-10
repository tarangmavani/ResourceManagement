using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(string? filter, string? sortBy, bool sortDescending, int page, int pageSize);
    Task<int> CountAsync(string? filter);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
