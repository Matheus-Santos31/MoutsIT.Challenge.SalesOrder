using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressHandler : IRequestHandler<SetDefaultUserAddressCommand, SetDefaultUserAddressResult>
{
    private readonly IUserAddressRepository _userAddressRepository;

    public SetDefaultUserAddressHandler(IUserAddressRepository userAddressRepository)
    {
        _userAddressRepository = userAddressRepository;
    }

    public async Task<SetDefaultUserAddressResult> Handle(SetDefaultUserAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new SetDefaultUserAddressValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != command.UserId)
            throw new UnauthorizedAccessException("You can only manage your own addresses.");

        var target = await _userAddressRepository.GetByIdAsync(command.AddressId, cancellationToken);
        if (target is null || target.UserId != command.UserId)
            throw new KeyNotFoundException($"Address with ID {command.AddressId} not found for this user");

        var all = await _userAddressRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        foreach (var userAddress in all.Where(x => x.IsDefault && x.Id != target.Id))
        {
            userAddress.IsDefault = false;
            await _userAddressRepository.UpdateAsync(userAddress, cancellationToken);
        }

        target.IsDefault = true;
        await _userAddressRepository.UpdateAsync(target, cancellationToken);
        await _userAddressRepository.SaveChangesAsync(cancellationToken);

        return new SetDefaultUserAddressResult
        {
            Id = target.Id,
            UserId = target.UserId,
            AddressId = target.AddressId,
            IsDefault = true
        };
    }
}
