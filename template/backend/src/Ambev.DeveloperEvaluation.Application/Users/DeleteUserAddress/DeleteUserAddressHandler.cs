using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUserAddress;

public class DeleteUserAddressHandler : IRequestHandler<DeleteUserAddressCommand, DeleteUserAddressResponse>
{
    private readonly IUserAddressRepository _userAddressRepository;

    public DeleteUserAddressHandler(IUserAddressRepository userAddressRepository)
    {
        _userAddressRepository = userAddressRepository;
    }

    public async Task<DeleteUserAddressResponse> Handle(DeleteUserAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeleteUserAddressValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != command.UserId)
            throw new UnauthorizedAccessException("You can only manage your own addresses.");

        var userAddress = await _userAddressRepository.GetByIdAsync(command.AddressId, cancellationToken);
        if (userAddress is null || userAddress.UserId != command.UserId)
            throw new KeyNotFoundException($"Address with ID {command.AddressId} not found for this user");

        await _userAddressRepository.DeleteAsync(userAddress, cancellationToken);
        await _userAddressRepository.SaveChangesAsync(cancellationToken);

        return new DeleteUserAddressResponse { Success = true };
    }
}
