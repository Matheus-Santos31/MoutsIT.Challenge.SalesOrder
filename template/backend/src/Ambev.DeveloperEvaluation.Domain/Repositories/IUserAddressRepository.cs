using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IUserAddressRepository : IBaseRepository<UserAddress>
{
    Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
