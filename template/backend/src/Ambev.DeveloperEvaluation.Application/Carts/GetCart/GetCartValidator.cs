using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartValidator : AbstractValidator<GetCartCommand>
{
    public GetCartValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
