namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Opt-in base for entities whose state changes must be published as integration events
/// via the transactional Outbox. Only entities that inherit from this 
/// participate <see cref="BaseEntity"/>.
///
/// The DbContext scans tracked <see cref="AggregateRoot"/> instances on every SaveChanges
/// call, turns any raised events into OutboxEvent rows in the very same
/// unit of work, then clears them — so the write to the aggregate and the write to the
/// outbox are always atomic, with no synchronous call to a message broker in the request path.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
