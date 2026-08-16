namespace Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public bool IsDefault { get; set; }
}
