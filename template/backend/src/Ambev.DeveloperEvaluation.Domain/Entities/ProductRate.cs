using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class ProductRate : BaseEntity
{
    public Guid ProductId { get; set; }
    public decimal AverageRate { get; set; }
    public int ReviewCount { get; set; }

    public Product? Product { get; set; }
}
