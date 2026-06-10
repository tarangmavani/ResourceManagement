using AutoMapper;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.ModifiedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
