using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.GetProduct;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string Image { get; set; } = string.Empty;
    public decimal? AverageRate { get; set; }
    public int? ReviewCount { get; set; }
}
