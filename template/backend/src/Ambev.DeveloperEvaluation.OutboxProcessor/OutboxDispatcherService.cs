using Ambev.DeveloperEvaluation.Common.Messaging;
using Ambev.DeveloperEvaluation.Common.ReadModels;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.OutboxProcessor;

/// <summary>
/// Polls the OutboxEvents table for Pending rows that are due, publishes each, and marks it as Sent or schedules a backoff retry.
/// </summary>
public class OutboxDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherService> _logger;
    private readonly OutboxProcessorOptions _options;

    public OutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherService> logger,
        IOptions<OutboxProcessorOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        do
        {
            try
            {
                await ProcessDueEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while processing outbox events");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessDueEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        var saleRepository = scope.ServiceProvider.GetRequiredService<ISaleRepository>();
        var readModelStore = scope.ServiceProvider.GetRequiredService<ISalesReadModelStore>();

        var dueEvents = (await outboxRepository.GetDueBatchAsync(_options.BatchSize, DateTime.UtcNow, cancellationToken)).ToList();
        if (dueEvents.Count == 0)
            return;

        _logger.LogInformation("Dispatching {Count} outbox event(s)", dueEvents.Count);

        foreach (var outboxEvent in dueEvents)
        {
            await DispatchAsync(outboxEvent, publisher, cancellationToken);

            if (outboxEvent.Status == OutboxEventStatus.Sent && outboxEvent.EntityType == nameof(Sale))
                await ProjectSalesReadModelAsync(outboxEvent, saleRepository, readModelStore, cancellationToken);

            await outboxRepository.UpdateAsync(outboxEvent, cancellationToken);
        }

        await outboxRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Re-projects the full Sale into the read model store after a Sale-related event was
    /// successfully dispatched. This is a downstream, eventually-consistent copy — Postgres
    /// stays the system of record, so a failure here never touches the outbox event's own
    /// status/retry bookkeeping, it's only logged.
    /// </summary>
    private async Task ProjectSalesReadModelAsync(
        OutboxEvent outboxEvent,
        ISaleRepository saleRepository,
        ISalesReadModelStore readModelStore,
        CancellationToken cancellationToken)
    {
        try
        {
            var sale = await saleRepository.GetByIdWithItemsAsync(outboxEvent.AggregateId, cancellationToken);
            if (sale is null)
                return;

            await readModelStore.UpsertAsync(ToReadModel(sale), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to project sales read model for outbox event {Id} (sale {SaleId})",
                outboxEvent.Id, outboxEvent.AggregateId);
        }
    }

    private static SaleHistoryDocument ToReadModel(Sale sale) => new()
    {
        SaleId = sale.Id,
        OrderId = sale.OrderId,
        UserId = sale.UserId,
        CustomerName = sale.CustomerName,
        CustomerEmail = sale.CustomerEmail,
        BranchId = sale.BranchId,
        BranchName = sale.BranchName,
        TotalAmount = sale.TotalAmount,
        ProductsQuantity = sale.ProductsQuantity,
        ItemsQuantity = sale.ItemsQuantity,
        TotalDiscount = sale.TotalDiscount,
        Status = sale.Status.ToString(),
        Items = sale.Items.Select(x => new SaleHistoryItemDocument
        {
            ProductId = x.ProductId,
            ProductTitle = x.ProductTitle,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Discount = x.Discount,
            TotalAmount = x.TotalAmount,
            Status = x.Status.ToString()
        }).ToList(),
        CreatedAt = sale.CreatedAt,
        ProjectedAt = DateTime.UtcNow
    };

    private async Task DispatchAsync(OutboxEvent outboxEvent, IMessagePublisher publisher, CancellationToken cancellationToken)
    {
        outboxEvent.LastAttemptAt = DateTime.UtcNow;

        try
        {
            await publisher.PublishAsync(outboxEvent.EventType, outboxEvent.Payload, cancellationToken);
            outboxEvent.Status = OutboxEventStatus.Sent;
            outboxEvent.LastError = null;
        }
        catch (Exception ex)
        {
            outboxEvent.RetryCount++;
            outboxEvent.LastError = ex.Message;

            if (outboxEvent.RetryCount >= _options.MaxRetries)
            {
                outboxEvent.Status = OutboxEventStatus.Failed;
                _logger.LogError(ex, "Outbox event {Id} ({EventType}) exceeded {MaxRetries} attempts, marking Failed",
                    outboxEvent.Id, outboxEvent.EventType, _options.MaxRetries);
            }
            else
            {
                var backoffSeconds = Math.Pow(2, outboxEvent.RetryCount) * _options.BackoffBaseSeconds;
                outboxEvent.NextAttemptAt = DateTime.UtcNow.AddSeconds(backoffSeconds);
                _logger.LogWarning(ex, "Failed to publish outbox event {Id} ({EventType}), attempt {RetryCount}/{MaxRetries}, retrying at {NextAttemptAt}",
                    outboxEvent.Id, outboxEvent.EventType, outboxEvent.RetryCount, _options.MaxRetries, outboxEvent.NextAttemptAt);
            }
        }
    }
}
