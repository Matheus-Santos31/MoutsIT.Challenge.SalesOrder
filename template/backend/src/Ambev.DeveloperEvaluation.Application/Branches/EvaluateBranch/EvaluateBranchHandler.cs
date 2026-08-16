using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.EvaluateBranch;

public class EvaluateBranchHandler : IRequestHandler<EvaluateBranchCommand, EvaluateBranchResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;
    private readonly IBranchRateRepository _branchRateRepository;

    public EvaluateBranchHandler(
        IBranchRepository branchRepository,
        IBranchEvaluationRepository branchEvaluationRepository,
        IBranchRateRepository branchRateRepository)
    {
        _branchRepository = branchRepository;
        _branchEvaluationRepository = branchEvaluationRepository;
        _branchRateRepository = branchRateRepository;
    }

    public async Task<EvaluateBranchResult> Handle(EvaluateBranchCommand command, CancellationToken cancellationToken)
    {
        var validator = new EvaluateBranchValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(command.BranchId, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {command.BranchId} not found");

        var evaluation = await _branchEvaluationRepository.GetByBranchAndUserAsync(command.BranchId, command.UserId, cancellationToken);
        if (evaluation is null)
        {
            evaluation = new BranchEvaluation
            {
                BranchId = command.BranchId,
                UserId = command.UserId,
                Rate = command.Rate,
                Comment = command.Comment
            };
            await _branchEvaluationRepository.AddAsync(evaluation, cancellationToken);
        }
        else
        {
            evaluation.Rate = command.Rate;
            evaluation.Comment = command.Comment;
            await _branchEvaluationRepository.UpdateAsync(evaluation, cancellationToken);
        }

        await _branchEvaluationRepository.SaveChangesAsync(cancellationToken);

        var allEvaluations = await _branchEvaluationRepository.GetAsync(x => x.BranchId == command.BranchId, cancellationToken);
        var evaluationList = allEvaluations.ToList();
        var averageRate = evaluationList.Average(x => x.Rate);
        var reviewCount = evaluationList.Count;

        var rate = await _branchRateRepository.GetByBranchIdAsync(command.BranchId, cancellationToken);
        if (rate is null)
        {
            rate = new BranchRate { BranchId = command.BranchId, AverageRate = averageRate, ReviewCount = reviewCount };
            await _branchRateRepository.AddAsync(rate, cancellationToken);
        }
        else
        {
            rate.AverageRate = averageRate;
            rate.ReviewCount = reviewCount;
            await _branchRateRepository.UpdateAsync(rate, cancellationToken);
        }

        await _branchRateRepository.SaveChangesAsync(cancellationToken);

        return new EvaluateBranchResult
        {
            Id = evaluation.Id,
            BranchId = evaluation.BranchId,
            UserId = evaluation.UserId,
            Rate = evaluation.Rate,
            Comment = evaluation.Comment,
            AverageRate = averageRate,
            ReviewCount = reviewCount
        };
    }
}
