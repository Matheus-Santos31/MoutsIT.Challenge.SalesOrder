using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CancelCart;

public class CancelCartCommand : IRequest<CancelCartResponse>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
