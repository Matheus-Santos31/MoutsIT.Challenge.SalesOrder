using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUserAddress;

public class DeleteUserAddressCommand : IRequest<DeleteUserAddressResponse>
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
