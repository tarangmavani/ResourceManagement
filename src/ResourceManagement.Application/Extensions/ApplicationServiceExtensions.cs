using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ResourceManagement.Application.Mappings;
using ResourceManagement.Application.Services;
using ResourceManagement.Application.Validators;

namespace ResourceManagement.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ProductMappingProfile).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IItemService, ItemService>();

        return services;
    }
}
