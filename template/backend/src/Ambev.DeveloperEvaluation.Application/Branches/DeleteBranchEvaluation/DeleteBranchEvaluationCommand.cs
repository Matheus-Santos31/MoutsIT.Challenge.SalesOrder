using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchEvaluation;

public record DeleteBranchEvaluationCommand(Guid BranchId, Guid EvaluationId) : IRequest<DeleteBranchEvaluationResponse>;
