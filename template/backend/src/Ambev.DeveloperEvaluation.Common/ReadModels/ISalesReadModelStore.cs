namespace Ambev.DeveloperEvaluation.Common.ReadModels;

/// <summary>
/// Port for the sale history read model store.
/// </summary>
public interface ISalesReadModelStore
{
    Task UpsertAsync(SaleHistoryDocument document, CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleHistoryDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SaleHistoryDocument?> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}
