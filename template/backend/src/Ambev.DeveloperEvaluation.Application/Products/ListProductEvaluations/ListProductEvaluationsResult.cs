namespace Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;

public class ListProductEvaluationsResult
{
    public IEnumerable<ProductEvaluationItemResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class ProductEvaluationItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
