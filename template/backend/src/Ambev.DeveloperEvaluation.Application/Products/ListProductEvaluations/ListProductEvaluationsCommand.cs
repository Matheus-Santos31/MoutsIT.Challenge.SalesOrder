using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;

public class ListProductEvaluationsCommand : IRequest<ListProductEvaluationsResult>
{
    public Guid ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
