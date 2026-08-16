using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

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
    /// Applies the README quantity-based discount tiers: no discount below 4 items,
    /// 10% for 4-9, 20% for 10-20, and a hard limit at 20 identical items.
    /// The cart doesn't reserve price: <paramref name="unitPrice"/> is not stored on the
    /// item, so <see cref="Discount"/>/<see cref="TotalAmount"/> only reflect the catalog
    /// price at the moment of this call. Callers that need up-to-date numbers for a cart
    /// still open (not yet checked out) should use <see cref="CalculatePricing"/> with the
    /// current Product.Price instead of trusting these persisted values.
    /// </summary>
    public void ApplyPricing(decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (quantity > 20)
            throw new DomainException("It's not possible to sell above 20 identical items.");

        Quantity = quantity;
        (Discount, TotalAmount) = CalculatePricing(unitPrice, quantity);
    }

    /// <summary>
    /// Pure calculation of the README quantity-based discount tiers, with no side effects.
    /// Used to reprice a cart item live against the current catalog price on read.
    /// </summary>
    public static (decimal Discount, decimal TotalAmount) CalculatePricing(decimal unitPrice, int quantity)
    {
        var discountPercentage = quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };

        var subtotal = unitPrice * quantity;
        var discount = subtotal * discountPercentage;
        return (discount, subtotal - discount);
    }
}
