using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;

public class GetBranchAddressHandler : IRequestHandler<GetBranchAddressCommand, GetBranchAddressResult>
{
    private readonly IBranchAddressRepository _branchAddressRepository;

    public GetBranchAddressHandler(IBranchAddressRepository branchAddressRepository)
    {
        _branchAddressRepository = branchAddressRepository;
    }

    public async Task<GetBranchAddressResult> Handle(GetBranchAddressCommand request, CancellationToken cancellationToken)
    {
        var branchAddress = await _branchAddressRepository.GetByBranchIdAsync(request.BranchId, cancellationToken);
        if (branchAddress is null)
            throw new KeyNotFoundException($"No address found for branch {request.BranchId}");

        return new GetBranchAddressResult
        {
            Id = branchAddress.Id,
            BranchId = branchAddress.BranchId,
            AddressId = branchAddress.AddressId,
            City = branchAddress.Address!.City,
            Street = branchAddress.Address.Street,
            Number = branchAddress.Address.Number,
            PostalCode = branchAddress.Address.PostalCode,
            Latitude = branchAddress.Address.Latitude,
            Longitude = branchAddress.Address.Longitude
        };
    }
}
