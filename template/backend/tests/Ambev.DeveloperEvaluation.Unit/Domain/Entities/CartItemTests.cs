using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Tests the README quantity-based discount tiers enforced by <see cref="CartItem.ApplyPricing"/>.
/// </summary>
public class CartItemTests
{
    [Theory(DisplayName = "Quantities below 4 get no discount")]
    [InlineData(1)]
    [InlineData(3)]
    public void Given_QuantityBelowFour_When_ApplyingPricing_Then_NoDiscountIsApplied(int quantity)
    {
        var item = new CartItem();

        item.ApplyPricing(unitPrice: 10m, quantity: quantity);

        Assert.Equal(0m, item.Discount);
        Assert.Equal(10m * quantity, item.TotalAmount);
    }

    [Theory(DisplayName = "Quantities from 4 to 9 get a 10% discount")]
    [InlineData(4)]
    [InlineData(9)]
    public void Given_QuantityBetweenFourAndNine_When_ApplyingPricing_Then_TenPercentDiscountIsApplied(int quantity)
    {
        var item = new CartItem();

        item.ApplyPricing(unitPrice: 10m, quantity: quantity);

        var subtotal = 10m * quantity;
        Assert.Equal(subtotal * 0.10m, item.Discount);
        Assert.Equal(subtotal - item.Discount, item.TotalAmount);
    }

    [Theory(DisplayName = "Quantities from 10 to 20 get a 20% discount")]
    [InlineData(10)]
    [InlineData(20)]
    public void Given_QuantityBetweenTenAndTwenty_When_ApplyingPricing_Then_TwentyPercentDiscountIsApplied(int quantity)
    {
        var item = new CartItem();

        item.ApplyPricing(unitPrice: 10m, quantity: quantity);

        var subtotal = 10m * quantity;
        Assert.Equal(subtotal * 0.20m, item.Discount);
        Assert.Equal(subtotal - item.Discount, item.TotalAmount);
    }

    [Fact(DisplayName = "Quantities above 20 are rejected")]
    public void Given_QuantityAboveTwenty_When_ApplyingPricing_Then_ThrowsDomainException()
    {
        var item = new CartItem();

        var act = () => item.ApplyPricing(unitPrice: 10m, quantity: 21);

        Assert.Throws<DomainException>(act);
    }

    [Fact(DisplayName = "Zero or negative quantities are rejected")]
    public void Given_ZeroQuantity_When_ApplyingPricing_Then_ThrowsDomainException()
    {
        var item = new CartItem();

        var act = () => item.ApplyPricing(unitPrice: 10m, quantity: 0);

        Assert.Throws<DomainException>(act);
    }
}
