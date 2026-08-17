using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchManagerRepository : BaseRepository<BranchManager>, IBranchManagerRepository
{
    private readonly DefaultContext _context;

    public BranchManagerRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<BranchManager?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchManagers.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsManagerOfBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchManagers.AnyAsync(x => x.UserId == userId && x.BranchId == branchId, cancellationToken);
    }
}
