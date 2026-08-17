using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IBranchManagerRepository : IBaseRepository<BranchManager>
{
    Task<BranchManager?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsManagerOfBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);
}
