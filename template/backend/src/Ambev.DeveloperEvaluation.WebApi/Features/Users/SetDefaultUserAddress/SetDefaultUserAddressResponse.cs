namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public bool IsDefault { get; set; }
}
