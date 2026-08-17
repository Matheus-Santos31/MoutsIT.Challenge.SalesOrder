using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

/// <summary>
/// Proves the core outbox mechanism: DefaultContext.SaveChangesAsync turns whatever domain
/// events a tracked AggregateRoot raised into OutboxEvent rows added to the very same unit
/// of work — no separate SaveChanges call, no synchronous call to a broker anywhere here.
/// </summary>
public class DefaultContextOutboxTests
{
    private static DefaultContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DefaultContext(options);
    }

    [Fact(DisplayName = "SaveChangesAsync turns a raised domain event into an OutboxEvent row in the same call")]
    public async Task SaveChangesAsync_AggregateWithDomainEvent_CreatesOutboxEventRow()
    {
        await using var context = BuildContext();

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            TotalAmount = 100m,
            Status = SaleStatus.Created
        };
        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id, sale.UserId, sale.BranchId, sale.TotalAmount));

        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        var outboxEvent = await context.OutboxEvents.SingleAsync();
        outboxEvent.EntityType.Should().Be(nameof(Sale));
        outboxEvent.AggregateId.Should().Be(sale.Id);
        outboxEvent.EventType.Should().Be(nameof(SaleCreatedEvent));
        outboxEvent.Status.Should().Be(OutboxEventStatus.Pending);
        outboxEvent.Payload.Should().Contain(sale.UserId.ToString());

        sale.DomainEvents.Should().BeEmpty("captured events are cleared right after being turned into outbox rows");
    }

    [Fact(DisplayName = "SaveChangesAsync leaves the outbox untouched when no domain events were raised")]
    public async Task SaveChangesAsync_AggregateWithoutDomainEvents_DoesNotCreateOutboxRow()
    {
        await using var context = BuildContext();

        var sale = new Sale { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid(), Status = SaleStatus.Created };
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        (await context.OutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact(DisplayName = "A sale raising two events produces two outbox rows in the same SaveChanges call")]
    public async Task SaveChangesAsync_MultipleDomainEvents_CreatesOneOutboxRowPerEvent()
    {
        await using var context = BuildContext();

        var sale = new Sale { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid(), Status = SaleStatus.Created };
        sale.AddDomainEvent(new SaleItemCancelledEvent(sale.Id, Guid.NewGuid(), Guid.NewGuid(), 3));
        sale.AddDomainEvent(new SaleCancelledEvent(sale.Id, sale.OrderId));

        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        var eventTypes = context.OutboxEvents.Select(x => x.EventType).ToList();
        eventTypes.Should().BeEquivalentTo([nameof(SaleItemCancelledEvent), nameof(SaleCancelledEvent)]);
    }
}
