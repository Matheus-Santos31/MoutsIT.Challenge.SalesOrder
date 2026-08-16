namespace Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;

public class EvaluateProductResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal AverageRate { get; set; }
    public int ReviewCount { get; set; }
}
