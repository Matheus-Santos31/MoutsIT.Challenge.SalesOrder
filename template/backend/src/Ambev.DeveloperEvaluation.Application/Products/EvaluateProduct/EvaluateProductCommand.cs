using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;

public class EvaluateProductCommand : IRequest<EvaluateProductResult>
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
