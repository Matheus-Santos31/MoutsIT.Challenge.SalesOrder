using Ambev.DeveloperEvaluation.Common.ReadModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ReadModel;

public class MongoSalesReadModelStore : ISalesReadModelStore
{
    private readonly IMongoCollection<SaleHistoryDocument> _collection;

    /// <summary>
    /// SaleHistoryDocument lives in Common and has no Id property (it must stay
    /// Mongo-agnostic — no MongoDB.Bson types leaking into the port's DTO). Every document
    /// still gets a server-generated "_id" on insert; without this, reading it back throws
    /// because the driver's default class map rejects BSON elements it can't match to a
    /// member. Registering the map here, in the adapter, keeps that Mongo-specific detail out
    /// of the shared DTO.
    /// </summary>
    static MongoSalesReadModelStore()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(SaleHistoryDocument)))
        {
            BsonClassMap.RegisterClassMap<SaleHistoryDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoSalesReadModelStore(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.Database);
        _collection = database.GetCollection<SaleHistoryDocument>("sales");
    }

    public async Task UpsertAsync(SaleHistoryDocument document, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SaleHistoryDocument>.Filter.Eq(x => x.SaleId, document.SaleId);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<IEnumerable<SaleHistoryDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SaleHistoryDocument>.Filter.Eq(x => x.UserId, userId);
        var sort = Builders<SaleHistoryDocument>.Sort.Descending(x => x.CreatedAt);
        return await _collection.Find(filter).Sort(sort).ToListAsync(cancellationToken);
    }

    public async Task<SaleHistoryDocument?> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SaleHistoryDocument>.Filter.Eq(x => x.SaleId, saleId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
