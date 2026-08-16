using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;

public class UpdateUserAddressCommand : IRequest<UpdateUserAddressResult>
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }

    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}
