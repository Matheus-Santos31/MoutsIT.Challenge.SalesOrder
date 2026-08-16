using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCartItem;

public class DeleteCartItemCommand : IRequest<DeleteCartItemResponse>
{
    public Guid CartId { get; set; }
    public Guid ItemId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
