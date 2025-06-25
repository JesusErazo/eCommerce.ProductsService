using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validators;

public class ProductAddRequestValidator: AbstractValidator<ProductAddRequest>
{
  public ProductAddRequestValidator()
  {
    //ProductName
    RuleFor(x => x.ProductName)
      .NotEmpty().WithMessage("Product name can't be blank");

    //Category
    RuleFor(x => x.Category)
      .IsInEnum().WithMessage("Category should be a valid type");

    //UnitPrice
    RuleFor(x => x.UnitPrice)
      .InclusiveBetween(0, double.MaxValue).WithMessage($"Unit price should be between 0 to {double.MaxValue}");

    //QuantityInStock
    RuleFor(x => x.QuantityInStock)
      .InclusiveBetween(0, int.MaxValue).WithMessage($"Quantity in stock should be between 0 to {int.MaxValue}");
  }
}
