using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid CartId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }

    public Product? Product { get; set; }
    public Cart? Cart { get; set; }
}
