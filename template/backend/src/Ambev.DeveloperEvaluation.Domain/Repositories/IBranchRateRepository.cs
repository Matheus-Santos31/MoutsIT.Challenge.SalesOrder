using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IBranchRateRepository : IBaseRepository<BranchRate>
{
    Task<BranchRate?> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
}
