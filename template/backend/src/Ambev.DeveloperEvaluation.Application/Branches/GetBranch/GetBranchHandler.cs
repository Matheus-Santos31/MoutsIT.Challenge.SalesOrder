using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranch;

public class GetBranchHandler : IRequestHandler<GetBranchCommand, GetBranchResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchRateRepository _branchRateRepository;
    private readonly IMapper _mapper;

    public GetBranchHandler(IBranchRepository branchRepository, IBranchRateRepository branchRateRepository, IMapper mapper)
    {
        _branchRepository = branchRepository;
        _branchRateRepository = branchRateRepository;
        _mapper = mapper;
    }

    public async Task<GetBranchResult> Handle(GetBranchCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetBranchValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {request.Id} not found");

        var result = _mapper.Map<GetBranchResult>(branch);

        var rate = await _branchRateRepository.GetByBranchIdAsync(request.Id, cancellationToken);
        if (rate is not null)
        {
            result.AverageRate = rate.AverageRate;
            result.ReviewCount = rate.ReviewCount;
        }

        return result;
    }
}
