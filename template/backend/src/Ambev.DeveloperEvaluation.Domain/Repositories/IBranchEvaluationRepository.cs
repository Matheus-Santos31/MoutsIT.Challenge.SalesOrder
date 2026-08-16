using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IBranchEvaluationRepository : IBaseRepository<BranchEvaluation>
{
    Task<BranchEvaluation?> GetByBranchAndUserAsync(Guid branchId, Guid userId, CancellationToken cancellationToken = default);
}
