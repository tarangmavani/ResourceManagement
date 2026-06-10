using AutoMapper;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ItemService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ItemDto>> GetByProductIdAsync(int productId)
    {
        var items = await _uow.Items.GetByProductIdAsync(productId);
        return _mapper.Map<IEnumerable<ItemDto>>(items);
    }

    public async Task<ItemDto?> GetByIdAsync(int id)
    {
        var item = await _uow.Items.GetByIdAsync(id);
        return item is null ? null : _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto?> CreateAsync(CreateItemDto dto)
    {
        // Verify the product exists
        var product = await _uow.Products.GetByIdAsync(dto.ProductId);
        if (product is null) return null;

        var item = _mapper.Map<Item>(dto);
        await _uow.Items.AddAsync(item);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto?> UpdateAsync(int id, UpdateItemDto dto)
    {
        var item = await _uow.Items.GetByIdAsync(id);
        if (item is null) return null;

        _mapper.Map(dto, item);
        await _uow.Items.UpdateAsync(item);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ItemDto>(item);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _uow.Items.GetByIdAsync(id);
        if (item is null) return false;

        await _uow.Items.DeleteAsync(item);
        await _uow.SaveChangesAsync();
        return true;
    }
}
