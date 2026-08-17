using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Common;

/// <summary>
/// Tests the shared quantity-based discount tiers used by both <see cref="Ambev.DeveloperEvaluation.Domain.Entities.CartItem"/>
/// (live pricing) and <see cref="Ambev.DeveloperEvaluation.Domain.Entities.SaleItem"/> (frozen at sale time).
/// </summary>
public class QuantityDiscountPolicyTests
{
    [Fact(DisplayName = "Calculate has no side effects and reflects whatever price is passed in")]
    public void Given_DifferentPrices_When_Calculating_Then_ReflectsEachPriceIndependently()
    {
        var (_, totalAmountAtOldPrice) = QuantityDiscountPolicy.Calculate(unitPrice: 15m, quantity: 2);
        var (_, totalAmountAtNewPrice) = QuantityDiscountPolicy.Calculate(unitPrice: 30m, quantity: 2);

        Assert.Equal(30m, totalAmountAtOldPrice);
        Assert.Equal(60m, totalAmountAtNewPrice);
    }

    [Fact(DisplayName = "Validate rejects quantities above the 20-item limit")]
    public void Given_QuantityAboveTwenty_When_Validating_Then_ThrowsDomainException()
    {
        var act = () => QuantityDiscountPolicy.Validate(21);

        Assert.Throws<DomainException>(act);
    }

    [Fact(DisplayName = "Validate rejects zero or negative quantities")]
    public void Given_ZeroQuantity_When_Validating_Then_ThrowsDomainException()
    {
        var act = () => QuantityDiscountPolicy.Validate(0);

        Assert.Throws<DomainException>(act);
    }
}
