using MediatR;
using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCartItem;

public class UpdateCartItemCommand : IRequest<CartItemResult>
{
    public Guid CartId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
