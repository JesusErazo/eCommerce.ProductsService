using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Extensions;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
  private readonly IMapper _mapper;
  private readonly IProductsRepository _productsRepository;
  private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
  private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
  public ProductsService(
    IMapper mapper, 
    IProductsRepository productsRepository,
    IValidator<ProductAddRequest> productAddRequestValidator,
    IValidator<ProductUpdateRequest> productUpdateRequestValidator
    )
  {
    _mapper = mapper;
    _productsRepository = productsRepository;
    _productAddRequestValidator = productAddRequestValidator;
    _productUpdateRequestValidator = productUpdateRequestValidator;
  }
  public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
  {
    if (productAddRequest is null)
    {
      throw new ArgumentNullException(nameof(productAddRequest));
    }
    
    ValidationResult result = await _productAddRequestValidator.ValidateAsync( productAddRequest );
    if (!result.IsValid) {
      string errors = result.Errors.ToBasicFormat().ToString();
      throw new ArgumentException(errors);
    }

    Product productInput = _mapper.Map<Product>(productAddRequest);

    Product? addedProduct = await _productsRepository.AddProduct( productInput );

    if (addedProduct is null) { return null; }

    return _mapper.Map<ProductResponse>(addedProduct);
  }

  public async Task<bool> DeleteProduct(Guid ProductID)
  {
    Product? existingProduct = await _productsRepository
      .GetProductByCondition(x => x.ProductID == ProductID);

    if (existingProduct is null) { return false; }

    return await _productsRepository.DeleteProduct( ProductID );
  }

  public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
  {
    Product? productDb = await _productsRepository.GetProductByCondition(conditionExpression);

    if (productDb is null) { return null; }

    return _mapper.Map<ProductResponse>(productDb);
  }

  public async Task<List<ProductResponse?>> GetProducts()
  {
    IEnumerable<Product?> productsDb = await _productsRepository.GetProducts();
    return _mapper.Map<List<ProductResponse?>>(productsDb);
  }

  public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
  {
    IEnumerable<Product?> productsDb = await _productsRepository.GetProductsByCondition(conditionExpression);

    if (productsDb is null){ return null; }

    return _mapper.Map<List<ProductResponse?>>(productsDb);
  }

  public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
  {
    if (productUpdateRequest is null)
    {
      throw new ArgumentNullException(nameof(productUpdateRequest));
    }

    ValidationResult result = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

    if (!result.IsValid)
    {
      string errors = result.Errors.ToBasicFormat().ToString();
      throw new ArgumentException(errors);
    }

    Product? existingProduct = await _productsRepository
      .GetProductByCondition(x => x.ProductID == productUpdateRequest.ProductID);

    if (existingProduct is null) {
      throw new ArgumentException("Invalid Product ID");
    }

    Product productNewData = _mapper.Map<Product>(productUpdateRequest);
    Product? updatedProduct = await _productsRepository.UpdateProduct(productNewData);

    if(updatedProduct is null) { return null; }

    return _mapper.Map<ProductResponse>(updatedProduct);
  }
}
