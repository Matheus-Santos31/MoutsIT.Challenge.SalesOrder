namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Marker for an event raised by an <see cref="AggregateRoot"/>. The concrete type name
/// (via reflection) becomes the Outbox row's EventType, and the event itself is
/// JSON-serialized into the Outbox row's Payload.
/// </summary>
public interface IDomainEvent
{
}
