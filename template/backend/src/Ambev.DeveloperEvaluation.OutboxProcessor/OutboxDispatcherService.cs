using Ambev.DeveloperEvaluation.Common.Messaging;
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

        var dueEvents = (await outboxRepository.GetDueBatchAsync(_options.BatchSize, DateTime.UtcNow, cancellationToken)).ToList();
        if (dueEvents.Count == 0)
            return;

        _logger.LogInformation("Dispatching {Count} outbox event(s)", dueEvents.Count);

        foreach (var outboxEvent in dueEvents)
        {
            await DispatchAsync(outboxEvent, publisher, cancellationToken);
            await outboxRepository.UpdateAsync(outboxEvent, cancellationToken);
        }

        await outboxRepository.SaveChangesAsync(cancellationToken);
    }

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
