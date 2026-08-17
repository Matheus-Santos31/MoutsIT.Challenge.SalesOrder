using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.AssignBranchManager;

/// <summary>
/// Assigns a user with the Manager role to a branch.
/// </summary>
public class AssignBranchManagerHandler : IRequestHandler<AssignBranchManagerCommand, AssignBranchManagerResponse>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;

    public AssignBranchManagerHandler(
        IBranchRepository branchRepository,
        IUserRepository userRepository,
        IBranchManagerRepository branchManagerRepository)
    {
        _branchRepository = branchRepository;
        _userRepository = userRepository;
        _branchManagerRepository = branchManagerRepository;
    }

    public async Task<AssignBranchManagerResponse> Handle(AssignBranchManagerCommand command, CancellationToken cancellationToken)
    {
        var validator = new AssignBranchManagerValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(command.BranchId, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {command.BranchId} not found");

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException($"User with ID {command.UserId} not found");

        if (user.Role != UserRole.Manager)
            throw new DomainException("Only users with the Manager role can be assigned to a branch.");

        var existing = await _branchManagerRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (existing is null)
        {
            await _branchManagerRepository.AddAsync(new BranchManager { BranchId = command.BranchId, UserId = command.UserId }, cancellationToken);
        }
        else
        {
            existing.BranchId = command.BranchId;
            await _branchManagerRepository.UpdateAsync(existing, cancellationToken);
        }

        await _branchManagerRepository.SaveChangesAsync(cancellationToken);

        return new AssignBranchManagerResponse { Success = true };
    }
}
