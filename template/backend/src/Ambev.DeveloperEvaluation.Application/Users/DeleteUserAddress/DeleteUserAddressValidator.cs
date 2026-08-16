using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUserAddress;

public class DeleteUserAddressValidator : AbstractValidator<DeleteUserAddressCommand>
{
    public DeleteUserAddressValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
