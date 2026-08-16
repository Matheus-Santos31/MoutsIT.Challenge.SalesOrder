namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.AddCartItem;

public class AddCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
