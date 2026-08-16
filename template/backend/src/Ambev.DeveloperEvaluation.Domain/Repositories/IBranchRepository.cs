using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IBranchRepository : IBaseRepository<Branch>
{
    Task<Branch?> GetByDocNumberAsync(string docNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
