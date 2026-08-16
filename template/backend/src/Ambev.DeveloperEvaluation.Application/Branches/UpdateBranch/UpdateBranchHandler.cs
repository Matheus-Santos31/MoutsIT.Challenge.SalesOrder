using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.UpdateBranch;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, UpdateBranchResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;

    public UpdateBranchHandler(IBranchRepository branchRepository, IMapper mapper)
    {
        _branchRepository = branchRepository;
        _mapper = mapper;
    }

    public async Task<UpdateBranchResult> Handle(UpdateBranchCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateBranchValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(command.Id, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {command.Id} not found");

        var existingBranch = await _branchRepository.GetByDocNumberAsync(command.DocNumber, command.Id, cancellationToken);
        if (existingBranch != null)
            throw new DomainException($"Branch with document number {command.DocNumber} already exists");

        branch.Name = command.Name;
        branch.DocNumber = command.DocNumber;
        branch.CompanyName = command.CompanyName;

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UpdateBranchResult>(branch);
    }
}
