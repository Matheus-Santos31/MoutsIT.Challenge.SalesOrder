using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.UnassignBranchManager;

public class UnassignBranchManagerHandler : IRequestHandler<UnassignBranchManagerCommand, UnassignBranchManagerResponse>
{
    private readonly IBranchManagerRepository _branchManagerRepository;

    public UnassignBranchManagerHandler(IBranchManagerRepository branchManagerRepository)
    {
        _branchManagerRepository = branchManagerRepository;
    }

    public async Task<UnassignBranchManagerResponse> Handle(UnassignBranchManagerCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _branchManagerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (mapping is null || mapping.BranchId != request.BranchId)
            throw new KeyNotFoundException($"User {request.UserId} is not a manager of branch {request.BranchId}");

        await _branchManagerRepository.DeleteAsync(mapping, cancellationToken);
        await _branchManagerRepository.SaveChangesAsync(cancellationToken);

        return new UnassignBranchManagerResponse { Success = true };
    }
}
