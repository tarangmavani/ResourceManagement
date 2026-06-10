using AutoMapper;
using ResourceManagement.Application.DTOs;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Application.Mappings;

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemDto>();
        CreateMap<CreateItemDto, Item>();
        CreateMap<UpdateItemDto, Item>();
    }
}
