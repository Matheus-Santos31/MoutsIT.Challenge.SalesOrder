using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Discount method. Shared by <see cref="Entities.CartItem"/>
/// <see cref="Entities.SaleItem"/> (frozen at sale time) so the tier math only lives in one place.
/// </summary>
public static class QuantityDiscountPolicy
{
    public const int MaxQuantity = 20;

    public static void Validate(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (quantity > MaxQuantity)
            throw new DomainException("It's not possible to sell above 20 identical items.");
    }

    public static (decimal Discount, decimal TotalAmount) Calculate(decimal unitPrice, int quantity)
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
