namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.EvaluateProduct;

public class EvaluateProductResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal AverageRate { get; set; }
    public int ReviewCount { get; set; }
}
