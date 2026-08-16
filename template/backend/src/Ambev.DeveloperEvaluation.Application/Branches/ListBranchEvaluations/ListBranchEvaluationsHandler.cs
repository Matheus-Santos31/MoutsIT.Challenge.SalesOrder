using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;

public class ListBranchEvaluationsHandler : IRequestHandler<ListBranchEvaluationsCommand, ListBranchEvaluationsResult>
{
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;

    public ListBranchEvaluationsHandler(IBranchEvaluationRepository branchEvaluationRepository)
    {
        _branchEvaluationRepository = branchEvaluationRepository;
    }

    public async Task<ListBranchEvaluationsResult> Handle(ListBranchEvaluationsCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListBranchEvaluationsValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (evaluations, totalCount) = await _branchEvaluationRepository.GetPagedAsync(
            command.Page, command.PageSize,
            filters: [x => x.BranchId == command.BranchId],
            cancellationToken: cancellationToken);

        var items = evaluations.Select(x => new BranchEvaluationItemResult
        {
            Id = x.Id,
            BranchId = x.BranchId,
            UserId = x.UserId,
            Rate = x.Rate,
            Comment = x.Comment
        });

        return new ListBranchEvaluationsResult { Items = items, TotalCount = totalCount };
    }
}
