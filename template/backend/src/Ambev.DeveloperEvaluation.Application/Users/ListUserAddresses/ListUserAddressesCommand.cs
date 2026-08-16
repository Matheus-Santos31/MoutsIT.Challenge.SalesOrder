using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;

public class ListUserAddressesCommand : IRequest<IEnumerable<UserAddressResult>>
{
    public Guid UserId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
