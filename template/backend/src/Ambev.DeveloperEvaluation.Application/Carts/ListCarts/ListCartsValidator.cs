using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsValidator : AbstractValidator<ListCartsCommand>
{
    public ListCartsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
