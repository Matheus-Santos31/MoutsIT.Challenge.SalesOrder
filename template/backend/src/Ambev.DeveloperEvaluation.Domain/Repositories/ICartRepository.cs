using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ICartRepository : IBaseRepository<Cart>
{
    Task<Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
