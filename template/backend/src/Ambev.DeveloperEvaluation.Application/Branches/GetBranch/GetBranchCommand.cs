using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranch;

public record GetBranchCommand(Guid Id) : IRequest<GetBranchResult>;
