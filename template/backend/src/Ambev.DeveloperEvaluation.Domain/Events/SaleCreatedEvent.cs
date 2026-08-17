using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCreatedEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public Guid UserId { get; }
    public Guid BranchId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public SaleCreatedEvent(Guid saleId, Guid userId, Guid branchId, decimal totalAmount)
    {
        SaleId = saleId;
        UserId = userId;
        BranchId = branchId;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}
