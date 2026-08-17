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

    /// <summary>
    /// The cart doesn't reserve price: <paramref name="unitPrice"/> is not stored on the
    /// item, so <see cref="Discount"/>/<see cref="TotalAmount"/> only reflect the catalog
    /// price at the moment of this call. Callers that need up-to-date numbers for a cart
    /// still open (not yet checked out) should call <see cref="QuantityDiscountPolicy.Calculate"/>
    /// with the current Product.Price instead of trusting these persisted values.
    /// </summary>
    public void ApplyPricing(decimal unitPrice, int quantity)
    {
        QuantityDiscountPolicy.Validate(quantity);

        Quantity = quantity;
        (Discount, TotalAmount) = QuantityDiscountPolicy.Calculate(unitPrice, quantity);
    }
}
