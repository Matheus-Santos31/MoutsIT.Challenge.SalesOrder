using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.ListCarts;

public class CartListItemResponse
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public CartStatus Status { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
}
