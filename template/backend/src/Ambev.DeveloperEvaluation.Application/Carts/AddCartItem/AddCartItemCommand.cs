using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;

public class AddCartItemCommand : IRequest<CartItemResult>
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
