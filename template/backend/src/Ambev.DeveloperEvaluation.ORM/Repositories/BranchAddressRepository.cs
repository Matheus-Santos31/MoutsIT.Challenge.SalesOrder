using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchAddressRepository : BaseRepository<BranchAddress>, IBranchAddressRepository
{
    private readonly DefaultContext _context;

    public BranchAddressRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<BranchAddress?> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchAddresses
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.BranchId == branchId, cancellationToken);
    }
}
