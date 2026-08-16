using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressCommand : IRequest<SetDefaultUserAddressResult>
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
