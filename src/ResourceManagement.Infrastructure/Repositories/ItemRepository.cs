using Microsoft.EntityFrameworkCore;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Infrastructure.Data;

namespace ResourceManagement.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetByProductIdAsync(int productId)
        => await _context.Items.Where(i => i.ProductId == productId).ToListAsync();

    public async Task<Item?> GetByIdAsync(int id)
        => await _context.Items.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Item> AddAsync(Item item)
    {
        _context.Items.Add(item);
        return item;
    }

    public async Task<Item> UpdateAsync(Item item)
    {
        _context.Items.Update(item);
        return item;
    }

    public async Task DeleteAsync(Item item)
        => _context.Items.Remove(item);
}
