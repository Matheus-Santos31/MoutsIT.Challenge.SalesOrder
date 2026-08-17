using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository : IBaseRepository<Sale>
{
    Task<Sale?> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
