using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchEvaluation;

public class DeleteBranchEvaluationHandler : IRequestHandler<DeleteBranchEvaluationCommand, DeleteBranchEvaluationResponse>
{
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;
    private readonly IBranchRateRepository _branchRateRepository;

    public DeleteBranchEvaluationHandler(IBranchEvaluationRepository branchEvaluationRepository, IBranchRateRepository branchRateRepository)
    {
        _branchEvaluationRepository = branchEvaluationRepository;
        _branchRateRepository = branchRateRepository;
    }

    public async Task<DeleteBranchEvaluationResponse> Handle(DeleteBranchEvaluationCommand request, CancellationToken cancellationToken)
    {
        var evaluation = await _branchEvaluationRepository.GetByIdAsync(request.EvaluationId, cancellationToken);
        if (evaluation is null || evaluation.BranchId != request.BranchId)
            throw new KeyNotFoundException($"Evaluation with ID {request.EvaluationId} not found for this branch");

        await _branchEvaluationRepository.DeleteAsync(evaluation, cancellationToken);
        await _branchEvaluationRepository.SaveChangesAsync(cancellationToken);

        var remaining = (await _branchEvaluationRepository.GetAsync(x => x.BranchId == request.BranchId, cancellationToken)).ToList();
        var rate = await _branchRateRepository.GetByBranchIdAsync(request.BranchId, cancellationToken);

        if (rate is not null)
        {
            if (remaining.Count == 0)
            {
                await _branchRateRepository.DeleteAsync(rate, cancellationToken);
            }
            else
            {
                rate.AverageRate = remaining.Average(x => x.Rate);
                rate.ReviewCount = remaining.Count;
                await _branchRateRepository.UpdateAsync(rate, cancellationToken);
            }

            await _branchRateRepository.SaveChangesAsync(cancellationToken);
        }

        return new DeleteBranchEvaluationResponse { Success = true };
    }
}
