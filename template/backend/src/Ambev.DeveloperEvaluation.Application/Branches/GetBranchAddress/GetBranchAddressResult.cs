namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;

public class GetBranchAddressResult
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid AddressId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}
