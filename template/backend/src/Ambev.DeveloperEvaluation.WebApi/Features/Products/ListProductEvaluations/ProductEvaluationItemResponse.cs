namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.ListProductEvaluations;

public class ProductEvaluationItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
