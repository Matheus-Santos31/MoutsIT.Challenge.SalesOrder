using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsValidator : AbstractValidator<ListProductsCommand>
{
    public ListProductsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
