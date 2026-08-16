using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
{
    private readonly DefaultContext _context;

    public CartItemRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId, cancellationToken);
    }
}
