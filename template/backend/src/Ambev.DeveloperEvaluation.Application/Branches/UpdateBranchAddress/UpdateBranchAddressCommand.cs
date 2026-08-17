using MediatR;
using Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;

namespace Ambev.DeveloperEvaluation.Application.Branches.UpdateBranchAddress;

public class UpdateBranchAddressCommand : IRequest<BranchAddressResult>
{
    public Guid BranchId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
