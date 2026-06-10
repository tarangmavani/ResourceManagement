using Microsoft.EntityFrameworkCore;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Data;

namespace ResourceManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? filter, string? sortBy, bool sortDescending, int page, int pageSize)
    {
        var query = _context.Products.Include(p => p.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(p => p.ProductName.Contains(filter));

        query = (sortBy?.ToLower()) switch
        {
            "productname" => sortDescending ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName),
            "createdon"   => sortDescending ? query.OrderByDescending(p => p.CreatedOn)   : query.OrderBy(p => p.CreatedOn),
            _             => sortDescending ? query.OrderByDescending(p => p.Id)           : query.OrderBy(p => p.Id)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? filter)
    {
        var query = _context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(p => p.ProductName.Contains(filter));
        return await query.CountAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return product;
    }

    public async Task DeleteAsync(Product product)
        => _context.Products.Remove(product);
}
