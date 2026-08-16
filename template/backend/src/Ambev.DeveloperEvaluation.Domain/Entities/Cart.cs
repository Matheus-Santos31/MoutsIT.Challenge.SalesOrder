using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Cart : BaseEntity
{
    public decimal TotalAmount { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public int TotalItems { get; set; }
    public CartStatus Status { get; set; }

    public Branch? Branch { get; set; }
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    public void RecalculateTotals(IEnumerable<CartItem> items)
    {
        var list = items.ToList();
        TotalItems = list.Sum(x => x.Quantity);
        TotalAmount = list.Sum(x => x.TotalAmount);
    }
}
