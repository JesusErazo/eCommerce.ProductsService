using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.DataAccessLayer.Entities;

namespace eCommerce.BusinessLogicLayer.Mappers;

public class ProductUpdateRequestToProductMappingProfile : Profile
{
  public ProductUpdateRequestToProductMappingProfile()
  {
    CreateMap<ProductUpdateRequest, Product>();
  }
}
