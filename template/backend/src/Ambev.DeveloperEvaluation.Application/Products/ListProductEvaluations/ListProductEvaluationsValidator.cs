using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;

public class ListProductEvaluationsValidator : AbstractValidator<ListProductEvaluationsCommand>
{
    public ListProductEvaluationsValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
