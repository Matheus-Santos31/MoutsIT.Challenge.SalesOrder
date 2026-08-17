using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// A domain event captured in the same transaction as the aggregate change that raised it (see <see cref="AggregateRoot"/>).
/// </summary>
public class OutboxEvent : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;

    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public OutboxEventStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public string? LastError { get; set; }
}
