using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchRepository : BaseRepository<Branch>, IBranchRepository
{
    private readonly DefaultContext _context;

    public BranchRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Branch?> GetByDocNumberAsync(string docNumber, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(x => x.DocNumber == docNumber && (excludeId == null || x.Id != excludeId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
