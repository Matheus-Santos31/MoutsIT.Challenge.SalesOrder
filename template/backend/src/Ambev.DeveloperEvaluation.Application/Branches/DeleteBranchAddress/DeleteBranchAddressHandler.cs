using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;

public class DeleteBranchAddressHandler : IRequestHandler<DeleteBranchAddressCommand, DeleteBranchAddressResponse>
{
    private readonly IBranchAddressRepository _branchAddressRepository;

    public DeleteBranchAddressHandler(IBranchAddressRepository branchAddressRepository)
    {
        _branchAddressRepository = branchAddressRepository;
    }

    public async Task<DeleteBranchAddressResponse> Handle(DeleteBranchAddressCommand request, CancellationToken cancellationToken)
    {
        var branchAddress = await _branchAddressRepository.GetByBranchIdAsync(request.BranchId, cancellationToken);
        if (branchAddress is null)
            throw new KeyNotFoundException($"No address found for branch {request.BranchId}");

        await _branchAddressRepository.DeleteAsync(branchAddress, cancellationToken);
        await _branchAddressRepository.SaveChangesAsync(cancellationToken);

        return new DeleteBranchAddressResponse { Success = true };
    }
}
