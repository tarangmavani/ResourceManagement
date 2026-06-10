using AutoMapper;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(string? filter, string? sortBy, bool sortDescending, int page, int pageSize)
    {
        var products = await _uow.Products.GetAllAsync(filter, sortBy, sortDescending, page, pageSize);
        var total = await _uow.Products.CountAsync(filter);

        return new PagedResult<ProductDto>
        {
            Data = _mapper.Map<IEnumerable<ProductDto>>(products),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Domain.Entities.Product>(dto);
        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product is null) return null;

        _mapper.Map(dto, product);
        await _uow.Products.UpdateAsync(product);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product is null) return false;

        await _uow.Products.DeleteAsync(product);
        await _uow.SaveChangesAsync();
        return true;
    }
}
