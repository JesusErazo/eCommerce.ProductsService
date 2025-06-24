using AutoMapper;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.BusinessLogicLayer.DTO;

namespace eCommerce.BusinessLogicLayer.Mappers;

public class ProductAddRequestToProductMappingProfile : Profile
{
  public ProductAddRequestToProductMappingProfile()
  {
    CreateMap<ProductAddRequest, Product>()
      .ForMember(dest => dest.ProductID, opt => opt.Ignore());
  }
}
