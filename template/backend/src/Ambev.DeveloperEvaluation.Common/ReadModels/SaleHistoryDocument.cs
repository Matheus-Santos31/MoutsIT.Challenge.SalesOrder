namespace Ambev.DeveloperEvaluation.Common.ReadModels;

/// <summary>
/// Denormalized, join-free projection of a Sale for the "sale history by customer" read model.
/// </summary>
public class SaleHistoryDocument
{
    public Guid SaleId { get; set; }
    public long OrderId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ProductsQuantity { get; set; }
    public int ItemsQuantity { get; set; }
    public decimal TotalDiscount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SaleHistoryItemDocument> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime ProjectedAt { get; set; }
}

public class SaleHistoryItemDocument
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
