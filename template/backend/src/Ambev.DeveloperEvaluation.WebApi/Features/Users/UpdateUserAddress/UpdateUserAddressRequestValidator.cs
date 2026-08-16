using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUserAddress;

public class UpdateUserAddressRequestValidator : AbstractValidator<UpdateUserAddressRequest>
{
    public UpdateUserAddressRequestValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).GreaterThan(0);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Latitude).MaximumLength(50);
        RuleFor(x => x.Longitude).MaximumLength(50);
    }
}
