using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Data;
using ResourceManagement.Infrastructure.Repositories;

namespace ResourceManagement.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IProductRepository? _products;
    private IItemRepository? _items;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products
        => _products ??= new ProductRepository(_context);

    public IItemRepository Items
        => _items ??= new ItemRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}
