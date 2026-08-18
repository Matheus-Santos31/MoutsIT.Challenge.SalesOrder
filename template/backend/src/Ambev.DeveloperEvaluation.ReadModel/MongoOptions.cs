namespace Ambev.DeveloperEvaluation.ReadModel;

/// <summary>
/// Binds to the "Mongo" configuration section.
/// </summary>
public class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "sales_order_read_models";
}
