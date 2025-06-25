using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.Context;
using eCommerce.DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eCommerce.DataAccessLayer.Repositories;

public class ProductsRepository : IProductsRepository
{
  private readonly ApplicationDbContext _dbContext;

  public ProductsRepository(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Product?> AddProduct(Product product)
  {
    _dbContext.Add(product);
    await _dbContext.SaveChangesAsync();
    return product;
  }

  public async Task<bool> DeleteProduct(Guid ProductID)
  {
    Product? existingProduct = await _dbContext.Products.FirstOrDefaultAsync(x => x.ProductID == ProductID);

    if (existingProduct is null)
    {
      return false;
    }

    _dbContext.Remove(existingProduct);
    int affectedRowsCount = await _dbContext.SaveChangesAsync();
    return affectedRowsCount > 0;
  }

  public async Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
  {
    return await _dbContext.Products.FirstOrDefaultAsync(conditionExpression);
  }

  public async Task<IEnumerable<Product?>> GetProducts()
  {
    return await _dbContext.Products.ToListAsync();
  }

  public async Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
  {
    return await _dbContext.Products.Where(conditionExpression).ToListAsync();
  }

  public async Task<Product?> UpdateProduct(Product product)
  {
    Product? existingProduct = await _dbContext.Products.FirstOrDefaultAsync(x => x.ProductID == product.ProductID);

    if (existingProduct is null) { return null; }

    existingProduct.ProductName = product.ProductName;
    existingProduct.Category = product.Category;
    existingProduct.UnitPrice = product.UnitPrice;
    existingProduct.QuantityInStock = product.QuantityInStock;

    await _dbContext.SaveChangesAsync();
    return existingProduct;
  }
}
