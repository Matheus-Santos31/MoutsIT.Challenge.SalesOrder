using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<DeleteBranchResponse>;
