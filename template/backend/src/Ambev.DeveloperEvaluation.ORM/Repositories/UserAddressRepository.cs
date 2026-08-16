using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class UserAddressRepository : BaseRepository<UserAddress>, IUserAddressRepository
{
    private readonly DefaultContext _context;

    public UserAddressRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .Include(x => x.Address)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .Include(x => x.Address)
            .Where(x => x.UserId == userId && x.IsDefault && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
