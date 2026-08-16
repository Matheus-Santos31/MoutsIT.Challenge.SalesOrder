using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsResult
{
    public IEnumerable<ProductListItemResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class ProductListItemResult
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
