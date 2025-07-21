using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.ProductsService.API.APIEndpoints;

public static class ProductAPIEndpoints
{
  public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
  {
    //GET /api/products
    app.MapGet("/api/products", async(IProductsService productsService) =>
    {
      List<ProductResponse?> products = await productsService.GetProducts();
      return Results.Ok(products);
    });

    //GET /api/products/search/product-id/{ProductID}
    app.MapGet("/api/products/search/product-id/{ProductID:guid}", async (IProductsService productsService, Guid ProductID) =>
    {
      ProductResponse? product = await productsService
      .GetProductByCondition(x => x.ProductID == ProductID);

      if(product is null) return Results.NotFound();

      return Results.Ok(product);
    });

    //GET /api/products/search/{SearchString}
    app.MapGet("/api/products/search/{SearchString}", async (IProductsService productsService, string SearchString) =>
    {
      List<ProductResponse?> productsByName = await productsService
      .GetProductsByCondition(x => 
      x.ProductName != null &&
      EF.Functions.Like(x.ProductName,$"%{SearchString}%")
      );

      List<ProductResponse?> productsByCategory = await productsService
      .GetProductsByCondition(x =>
      x.Category != null &&
      EF.Functions.Like(x.Category, $"%{SearchString}%")
      );

      var products = productsByName.Union(productsByCategory);

      return Results.Ok(products);
    });

    //GET /api/products/search
    app.MapGet("/api/products/search", async (
      [FromServices] IProductsService productsService, 
      [FromQuery] Guid[] ids) =>
    {
      if (ids.Length < 1) {
        return Results.BadRequest("At least one 'ids' query param is required.");
      }

      List<ProductResponse?> products = await productsService
      .GetProductsByCondition(x =>
        ids.Contains(x.ProductID)
      );

      return Results.Ok(products);
    });

    //POST /api/products
    app.MapPost("/api/products", async (
      IProductsService productsService,
      IValidator<ProductAddRequest> productAddRequestValidator,
      ProductAddRequest productAddRequest
      ) =>
    {
      ValidationResult result = await productAddRequestValidator.ValidateAsync(productAddRequest);

      if (!result.IsValid) {
        Dictionary<string, string[]> errors = result.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
      }

      ProductResponse? addedProductResponse = await productsService.AddProduct(productAddRequest);

      if(addedProductResponse is null)
      {
        return Results.Problem("Error in adding product");
      }

      return Results.Created($"/api/products/search/product-id/{addedProductResponse.ProductID}", addedProductResponse);

    });

    //PUT /api/products
    app.MapPut("/api/products", async (
      IProductsService productsService,
      IValidator<ProductUpdateRequest> productUpdateRequestValidator,
      ProductUpdateRequest productUpdateRequest
      ) =>
    {

      ValidationResult result = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

      if (!result.IsValid) {
        Dictionary<string, string[]> errors = result.Errors
        .GroupBy(err => err.PropertyName)
        .ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
      }

      ProductResponse? updatedProduct = await productsService.UpdateProduct(productUpdateRequest);

      if (updatedProduct is null)
      {
        return Results.Problem("Error in updating product");
      }

      return Results.Ok(updatedProduct);

    });

    //DELETE /api/products/{producID:guid}
    app.MapDelete("/api/products/{productID:guid}", async (
      IProductsService productsService,
      Guid productID
      ) =>
    {

      bool isDeleted = await productsService.DeleteProduct(productID);

      if (!isDeleted)
      {
        return Results.Problem("Error in deleting product");
      }

      return Results.Ok(true);

    });

    return app;
  }
}
