using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : AggregateRoot
{
    /// <summary>
    /// The cart this sale was created from.
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Sale code more human-facing
    /// </summary>
    public long OrderId { get; set; }

    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;
    public string BranchDocNumber { get; set; } = string.Empty;
    public string BranchCompanyName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    /// <summary>Number of distinct products in the sale.</summary>
    public int ProductsQuantity { get; set; }

    /// <summary>Total number of items across all products (sum of SaleItem.Quantity).</summary>
    public int ItemsQuantity { get; set; }

    public decimal TotalDiscount { get; set; }
    public SaleStatus Status { get; set; }

    /// <summary>Snapshot of the customer's address at the time of the sale.</summary>
    public SaleAddress CustomerAddress { get; set; } = new();

    /// <summary>Snapshot of the branch's address at the time of the sale.</summary>
    public SaleAddress BranchAddress { get; set; } = new();

    public Cart? Cart { get; set; }
    public User? User { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    /// <summary>
    /// Recompute tha sale's rolloups from currently actives items only.
    /// </summary>
    public void RecalculateTotals()
    {
        var activeItems = Items.Where(x => x.Status == SaleItemStatus.Active).ToList();

        ProductsQuantity = activeItems.Count;
        ItemsQuantity = activeItems.Sum(x => x.Quantity);
        TotalDiscount = activeItems.Sum(x => x.Discount);
        TotalAmount = activeItems.Sum(x => x.TotalAmount);
    }
}
