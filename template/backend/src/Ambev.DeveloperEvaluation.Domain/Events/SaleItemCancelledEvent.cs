using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleItemCancelledEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public Guid SaleItemId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public DateTime OccurredAt { get; }

    public SaleItemCancelledEvent(Guid saleId, Guid saleItemId, Guid productId, int quantity)
    {
        SaleId = saleId;
        SaleItemId = saleItemId;
        ProductId = productId;
        Quantity = quantity;
        OccurredAt = DateTime.UtcNow;
    }
}
