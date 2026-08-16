using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.CreateProduct;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Category).NotEqual(ProductCategory.Unspecified);
        RuleFor(x => x.Image).MaximumLength(500);
    }
}
