using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsCommand : IRequest<ListCartsResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? BranchId { get; set; }
    public CartStatus? Status { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
    public bool IsRequestingUserManager { get; set; }
}
