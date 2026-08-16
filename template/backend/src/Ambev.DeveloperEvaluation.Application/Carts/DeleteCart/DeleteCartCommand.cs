using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public class DeleteCartCommand : IRequest<DeleteCartResponse>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
