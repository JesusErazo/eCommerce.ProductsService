namespace eCommerce.BusinessLogicLayer.DTO;

public record ValidationFailureItem(
  string propertyName,
  string errorMessage
  );
