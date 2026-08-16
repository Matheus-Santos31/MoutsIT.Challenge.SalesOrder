namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUserAddress;

public class CreateUserAddressRequest
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
