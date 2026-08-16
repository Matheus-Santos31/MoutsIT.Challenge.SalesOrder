using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class ProductEvaluationRepository : BaseRepository<ProductEvaluation>, IProductEvaluationRepository
{
    private readonly DefaultContext _context;

    public ProductEvaluationRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ProductEvaluation?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductEvaluations
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == userId, cancellationToken);
    }
}
