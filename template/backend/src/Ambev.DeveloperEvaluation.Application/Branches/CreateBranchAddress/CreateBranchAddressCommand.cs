using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;

public class CreateBranchAddressCommand : IRequest<BranchAddressResult>
{
    public Guid BranchId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}
