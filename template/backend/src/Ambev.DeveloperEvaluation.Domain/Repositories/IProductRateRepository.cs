using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IProductRateRepository : IBaseRepository<ProductRate>
{
    Task<ProductRate?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
