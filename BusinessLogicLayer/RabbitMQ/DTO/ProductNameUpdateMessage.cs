namespace eCommerce.BusinessLogicLayer.RabbitMQ.DTO;

public record ProductNameUpdateMessage(
  Guid ProductID, string? NewName);
