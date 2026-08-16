using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchRateRepository : BaseRepository<BranchRate>, IBranchRateRepository
{
    private readonly DefaultContext _context;

    public BranchRateRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<BranchRate?> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchRates.FirstOrDefaultAsync(x => x.BranchId == branchId, cancellationToken);
    }
}
