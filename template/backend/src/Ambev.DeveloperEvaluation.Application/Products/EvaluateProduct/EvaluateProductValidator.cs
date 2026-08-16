using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;

public class EvaluateProductValidator : AbstractValidator<EvaluateProductCommand>
{
    public EvaluateProductValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rate).InclusiveBetween(0, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
