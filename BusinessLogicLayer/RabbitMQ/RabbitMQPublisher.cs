using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace eCommerce.BusinessLogicLayer.RabbitMQ;

public class RabbitMQPublisher : IRabbitMQPublisher, IAsyncDisposable
{
  private readonly string _exchangeProductsName = Environment.GetEnvironmentVariable("RABBITMQ_PRODUCTS_EXCHANGE")
    ?? throw new NullReferenceException("RABBITMQ_PRODUCTS_EXCHANGE is missing.");

  private readonly IConfiguration _configuration;
  private readonly ILogger<RabbitMQPublisher> _logger;
  private IConnection? _connection;
  private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

  public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  private async Task<IConnection> GetConnectionAsync()
  {
    if (_connection is not null)
    {
      return _connection;
    }

    await _connectionLock.WaitAsync();

    try
    {
      //This is a Double-check locking pattern, please don't remove it.
      if(_connection is not null)
      {
        return _connection;
      }

      string hostname = _configuration["RABBITMQ_HOST"] 
        ?? throw new ArgumentNullException("RABBITMQ_HOST is missing");

      string port = _configuration["RABBITMQ_PORT"]
        ?? throw new ArgumentNullException("RABBITMQ_PORT is missing");

      string user = _configuration["RABBITMQ_USER"]
        ?? throw new ArgumentNullException("RABBITMQ_USER is missing");

      string password = _configuration["RABBITMQ_PASSWORD"]
        ?? throw new ArgumentNullException("RABBITMQ_PASSWORD is missing");


      ConnectionFactory connectionFactory = new ConnectionFactory()
      {
        HostName = hostname,
        Port = int.Parse(port),
        UserName = user,
        Password = password,
        //Good practice for containerized envs
        AutomaticRecoveryEnabled = true
      };

      _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", hostname, port);
      _connection = await connectionFactory.CreateConnectionAsync();

      return _connection;
    }
    finally
    {
      _connectionLock.Release();
    }
  }

  public async Task PublishAsync<T>(string routingKey, T message)
  {
    try
    {
      IConnection connection = await GetConnectionAsync();
      using var channel = await connection.CreateChannelAsync();

      //TO DO: Create exchanges, queues and bindings since rabbitmq.conf file
      await channel.ExchangeDeclareAsync(
        exchange: _exchangeProductsName,
        type: ExchangeType.Direct,
        durable: true);

      await channel.QueueDeclareAsync(
        queue: routingKey,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);

      await channel.QueueBindAsync(
        queue: routingKey,
        exchange: _exchangeProductsName,
        routingKey: routingKey);

      string json = JsonSerializer.Serialize(message);
      byte[] body = Encoding.UTF8.GetBytes(json);

      BasicProperties properties = new BasicProperties
      {
        Persistent = true
      };

      await channel.BasicPublishAsync(
        exchange: _exchangeProductsName,
        routingKey: routingKey,
        mandatory: true,
        basicProperties: properties,
        body: body);

      _logger.LogInformation("Message published to queue '{Queue}'", routingKey);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing message to RabbitMQ");
    }
  }

  //Cleanup resources
  public async ValueTask DisposeAsync()
  {
    if(_connection is not null)
    {
      await _connection.CloseAsync();
      await _connection.DisposeAsync();
    }
  }
}
