namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.EvaluateProduct;

public class EvaluateProductRequest
{
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
