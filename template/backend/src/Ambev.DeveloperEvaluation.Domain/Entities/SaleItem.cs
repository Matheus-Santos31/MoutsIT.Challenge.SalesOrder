using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }

    /// <summary>
    /// Snapshot of the product's descriptive data at the time of the sale (External Identities
    /// pattern with denormalization) so a later rename, recategorization or deletion of the
    /// product never changes what this sale record says was actually sold.
    /// </summary>
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public ProductCategory ProductCategory { get; set; }

    /// <summary>
    /// Unlike CartItem, this is frozen at the moment of sale: unlike a cart, a completed
    /// sale must never reflect a later catalog price change.
    /// </summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleItemStatus Status { get; set; }

    public Sale? Sale { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// Snapshots the product's descriptive data and freezes pricing using the README's
    /// quantity-based discount tiers. Once applied, later Product/price changes cannot
    /// affect this item.
    /// </summary>
    public void ApplyPricing(Product product, int quantity)
    {
        QuantityDiscountPolicy.Validate(quantity);

        ProductId = product.Id;
        ProductTitle = product.Title;
        ProductDescription = product.Description;
        ProductCategory = product.Category;
        UnitPrice = product.Price;
        Quantity = quantity;
        Status = SaleItemStatus.Active;
        (Discount, TotalAmount) = QuantityDiscountPolicy.Calculate(product.Price, quantity);
    }
}
