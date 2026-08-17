using Ambev.DeveloperEvaluation.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.MessageBus;

/// <summary>
/// No-op publisher that just logs — the README explicitly allows logging instead of
/// publishing for real. Also doubles as a local-dev fallback when no broker is available.
/// </summary>
public class LoggingMessagePublisher : IMessagePublisher
{
    private readonly ILogger<LoggingMessagePublisher> _logger;

    public LoggingMessagePublisher(ILogger<LoggingMessagePublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[LoggingMessagePublisher] {EventType}: {Payload}", eventType, payload);
        return Task.CompletedTask;
    }
}
