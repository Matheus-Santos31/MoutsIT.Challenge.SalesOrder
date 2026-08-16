using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.EvaluateProduct;

public class EvaluateProductRequestValidator : AbstractValidator<EvaluateProductRequest>
{
    public EvaluateProductRequestValidator()
    {
        RuleFor(x => x.Rate).InclusiveBetween(0, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
