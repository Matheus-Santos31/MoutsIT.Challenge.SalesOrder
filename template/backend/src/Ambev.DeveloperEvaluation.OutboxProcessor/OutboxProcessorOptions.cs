namespace Ambev.DeveloperEvaluation.OutboxProcessor;

public class OutboxProcessorOptions
{
    public const string SectionName = "OutboxProcessor";

    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
    public int MaxRetries { get; set; } = 5;
    public double BackoffBaseSeconds { get; set; } = 2;
}
