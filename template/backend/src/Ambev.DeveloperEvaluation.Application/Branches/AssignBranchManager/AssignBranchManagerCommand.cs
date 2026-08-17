using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.AssignBranchManager;

public class AssignBranchManagerCommand : IRequest<AssignBranchManagerResponse>
{
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
}
