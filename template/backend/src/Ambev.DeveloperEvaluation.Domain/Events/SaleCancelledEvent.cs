using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCancelledEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public long OrderId { get; }
    public DateTime OccurredAt { get; }

    public SaleCancelledEvent(Guid saleId, long orderId)
    {
        SaleId = saleId;
        OrderId = orderId;
        OccurredAt = DateTime.UtcNow;
    }
}
