namespace eCommerce.BusinessLogicLayer.DTO;

public record ValidationFailureResponse(
  List<ValidationFailureItem> errors
  );
