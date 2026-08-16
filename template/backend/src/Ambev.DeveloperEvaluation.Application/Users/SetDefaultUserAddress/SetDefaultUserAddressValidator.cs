using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressValidator : AbstractValidator<SetDefaultUserAddressCommand>
{
    public SetDefaultUserAddressValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
