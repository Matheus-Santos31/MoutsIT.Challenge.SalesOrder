using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IProductEvaluationRepository : IBaseRepository<ProductEvaluation>
{
    Task<ProductEvaluation?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken cancellationToken = default);
}
