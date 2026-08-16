using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchEvaluationRepository : BaseRepository<BranchEvaluation>, IBranchEvaluationRepository
{
    private readonly DefaultContext _context;

    public BranchEvaluationRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<BranchEvaluation?> GetByBranchAndUserAsync(Guid branchId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchEvaluations
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.UserId == userId, cancellationToken);
    }
}
