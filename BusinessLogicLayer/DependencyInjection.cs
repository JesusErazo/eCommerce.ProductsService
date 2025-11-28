using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
  public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
  {
    //TO DO: Add Business Logic Layer services to the IoC container.
    services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
    services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);
    services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();
    services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
    return services;
  }
}
