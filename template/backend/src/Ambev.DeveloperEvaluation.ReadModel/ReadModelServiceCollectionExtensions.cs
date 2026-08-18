using Ambev.DeveloperEvaluation.Common.ReadModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.ReadModel;

public static class ReadModelServiceCollectionExtensions
{
    /// <summary>
    /// Registers MongoSalesReadModelStore
    /// </summary>
    public static IServiceCollection AddReadModel(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.AddSingleton<ISalesReadModelStore, MongoSalesReadModelStore>();

        return services;
    }
}
