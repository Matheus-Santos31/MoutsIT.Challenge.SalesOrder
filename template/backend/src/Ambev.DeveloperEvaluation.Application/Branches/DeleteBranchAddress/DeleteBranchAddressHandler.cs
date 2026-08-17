using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;

public class DeleteBranchAddressHandler : IRequestHandler<DeleteBranchAddressCommand, DeleteBranchAddressResponse>
{
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;

    public DeleteBranchAddressHandler(IBranchAddressRepository branchAddressRepository, IBranchManagerRepository branchManagerRepository)
    {
        _branchAddressRepository = branchAddressRepository;
        _branchManagerRepository = branchManagerRepository;
    }

    public async Task<DeleteBranchAddressResponse> Handle(DeleteBranchAddressCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsRequestingUserAdmin
            && !await _branchManagerRepository.IsManagerOfBranchAsync(request.RequestingUserId, request.BranchId, cancellationToken))
            throw new UnauthorizedAccessException("You can only manage branches you're assigned to.");

        var branchAddress = await _branchAddressRepository.GetByBranchIdAsync(request.BranchId, cancellationToken);
        if (branchAddress is null)
            throw new KeyNotFoundException($"No address found for branch {request.BranchId}");

        await _branchAddressRepository.DeleteAsync(branchAddress, cancellationToken);
        await _branchAddressRepository.SaveChangesAsync(cancellationToken);

        return new DeleteBranchAddressResponse { Success = true };
    }
}
