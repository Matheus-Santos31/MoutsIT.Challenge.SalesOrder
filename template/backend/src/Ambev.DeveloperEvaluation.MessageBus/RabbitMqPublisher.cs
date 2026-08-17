using System.Text;
using Ambev.DeveloperEvaluation.Common.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ambev.DeveloperEvaluation.MessageBus;

/// <summary>
/// Publishes to a durable topic exchange, routing key = event type name.
/// </summary>
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly Lazy<IConnection> _connection;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _connection = new Lazy<IConnection>(CreateConnection);
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        return factory.CreateConnection("ambev-outbox-processor");
    }

    public Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        using var channel = _connection.Value.CreateModel();

        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(payload);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(_options.ExchangeName, routingKey: eventType, basicProperties: properties, body: body);

        _logger.LogInformation("Published {EventType} to exchange {Exchange}", eventType, _options.ExchangeName);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_connection.IsValueCreated)
            return;

        _connection.Value.Close();
        _connection.Value.Dispose();
    }
}
