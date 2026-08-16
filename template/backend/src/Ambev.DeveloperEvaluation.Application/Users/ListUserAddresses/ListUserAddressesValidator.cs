using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;

public class ListUserAddressesValidator : AbstractValidator<ListUserAddressesCommand>
{
    public ListUserAddressesValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
