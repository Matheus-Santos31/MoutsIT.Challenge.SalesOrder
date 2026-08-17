using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.UnassignBranchManager;

public record UnassignBranchManagerCommand(Guid BranchId, Guid UserId) : IRequest<UnassignBranchManagerResponse>;
