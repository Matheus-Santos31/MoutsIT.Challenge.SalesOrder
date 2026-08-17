using Ambev.DeveloperEvaluation.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.MessageBus;

public static class MessageBusServiceCollectionExtensions
{
    /// <summary>
    /// Registers RabbitMqPublisher
    /// </summary>
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        var provider = configuration["MessageBus:Provider"] ?? "RabbitMQ";

        if (string.Equals(provider, "Logging", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IMessagePublisher, LoggingMessagePublisher>();
        else
            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
