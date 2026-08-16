using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.EvaluateBranch;

public class EvaluateBranchCommand : IRequest<EvaluateBranchResult>
{
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
