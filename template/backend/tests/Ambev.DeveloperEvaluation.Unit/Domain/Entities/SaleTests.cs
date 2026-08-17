using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Tests <see cref="Sale.RecalculateTotals"/> — the rollup always reflects Active items only.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "RecalculateTotals sums only Active items, ignoring Cancelled ones")]
    public void Given_MixOfActiveAndCancelledItems_When_Recalculating_Then_IgnoresCancelledItems()
    {
        var sale = new Sale
        {
            Items =
            [
                new SaleItem { Quantity = 2, Discount = 0m, TotalAmount = 20m, Status = SaleItemStatus.Active },
                new SaleItem { Quantity = 5, Discount = 5m, TotalAmount = 45m, Status = SaleItemStatus.Cancelled },
                new SaleItem { Quantity = 1, Discount = 0m, TotalAmount = 5m, Status = SaleItemStatus.Active }
            ]
        };

        sale.RecalculateTotals();

        Assert.Equal(2, sale.ProductsQuantity);
        Assert.Equal(3, sale.ItemsQuantity);
        Assert.Equal(0m, sale.TotalDiscount);
        Assert.Equal(25m, sale.TotalAmount);
    }

    [Fact(DisplayName = "RecalculateTotals zeroes out when every item is Cancelled")]
    public void Given_AllItemsCancelled_When_Recalculating_Then_TotalsAreZero()
    {
        var sale = new Sale
        {
            Items =
            [
                new SaleItem { Quantity = 2, Discount = 0m, TotalAmount = 20m, Status = SaleItemStatus.Cancelled }
            ]
        };

        sale.RecalculateTotals();

        Assert.Equal(0, sale.ProductsQuantity);
        Assert.Equal(0, sale.ItemsQuantity);
        Assert.Equal(0m, sale.TotalDiscount);
        Assert.Equal(0m, sale.TotalAmount);
    }
}
