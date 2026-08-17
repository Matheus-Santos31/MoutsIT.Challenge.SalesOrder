using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await _context.Sales.FirstOrDefaultAsync(x => x.CartId == cartId, cancellationToken);
    }

    public async Task<Sale?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
