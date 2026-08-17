namespace Ambev.DeveloperEvaluation.Common.Messaging;

/// <summary>
/// Publishes an already-serialized message to whatever broker is configured.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes <paramref name="payload"/> (already-serialized, e.g. JSON) under
    /// <paramref name="eventType"/> as the routing key/topic name.
    /// </summary>
    Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default);
}
