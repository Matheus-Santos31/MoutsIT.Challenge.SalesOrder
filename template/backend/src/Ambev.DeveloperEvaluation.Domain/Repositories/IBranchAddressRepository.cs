using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IBranchAddressRepository : IBaseRepository<BranchAddress>
{
    Task<BranchAddress?> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
}
