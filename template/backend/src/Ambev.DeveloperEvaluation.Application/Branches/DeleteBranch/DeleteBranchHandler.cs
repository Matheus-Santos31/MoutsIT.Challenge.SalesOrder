using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranch;

public class DeleteBranchHandler : IRequestHandler<DeleteBranchCommand, DeleteBranchResponse>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;

    public DeleteBranchHandler(IBranchRepository branchRepository, IBranchManagerRepository branchManagerRepository)
    {
        _branchRepository = branchRepository;
        _branchManagerRepository = branchManagerRepository;
    }

    public async Task<DeleteBranchResponse> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteBranchValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {request.Id} not found");

        if (!request.IsRequestingUserAdmin
            && !await _branchManagerRepository.IsManagerOfBranchAsync(request.RequestingUserId, branch.Id, cancellationToken))
            throw new UnauthorizedAccessException("You can only manage branches you're assigned to.");

        await _branchRepository.DeleteAsync(branch, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        return new DeleteBranchResponse { Success = true };
    }
}
