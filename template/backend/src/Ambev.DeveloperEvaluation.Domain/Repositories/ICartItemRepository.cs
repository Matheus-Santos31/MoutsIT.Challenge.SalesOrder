using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ICartItemRepository : IBaseRepository<CartItem>
{
    Task<CartItem?> GetByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default);
}
