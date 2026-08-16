using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Cart : BaseEntity
{
    public decimal TotalAmount { get; set; }
    public Guid BranchId { get; set; }
    public int TotalItems { get; set; }
    public CartStatus Status { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
