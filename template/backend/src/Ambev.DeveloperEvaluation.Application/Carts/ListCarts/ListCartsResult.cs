using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsResult
{
    public IEnumerable<CartListItemResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class CartListItemResult
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public CartStatus Status { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
}
