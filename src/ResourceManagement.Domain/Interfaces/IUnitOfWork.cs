namespace ResourceManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IItemRepository Items { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
