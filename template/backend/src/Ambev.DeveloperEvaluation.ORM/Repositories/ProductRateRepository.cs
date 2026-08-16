using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class ProductRateRepository : BaseRepository<ProductRate>, IProductRateRepository
{
    private readonly DefaultContext _context;

    public ProductRateRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ProductRate?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductRates.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }
}
