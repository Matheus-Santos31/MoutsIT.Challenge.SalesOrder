using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;

public class ListBranchEvaluationsCommand : IRequest<ListBranchEvaluationsResult>
{
    public Guid BranchId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
