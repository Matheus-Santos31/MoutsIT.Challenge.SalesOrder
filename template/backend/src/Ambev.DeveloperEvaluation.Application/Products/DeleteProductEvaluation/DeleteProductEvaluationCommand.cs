using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProductEvaluation;

public record DeleteProductEvaluationCommand(Guid ProductId, Guid EvaluationId) : IRequest<DeleteProductEvaluationResponse>;
