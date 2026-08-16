using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;

public class UpdateUserAddressHandler : IRequestHandler<UpdateUserAddressCommand, UpdateUserAddressResult>
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IAddressRepository _addressRepository;

    public UpdateUserAddressHandler(IUserAddressRepository userAddressRepository, IAddressRepository addressRepository)
    {
        _userAddressRepository = userAddressRepository;
        _addressRepository = addressRepository;
    }

    public async Task<UpdateUserAddressResult> Handle(UpdateUserAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateUserAddressValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != command.UserId)
            throw new UnauthorizedAccessException("You can only manage your own addresses.");

        var userAddress = await _userAddressRepository.GetByIdAsync(command.AddressId, cancellationToken);
        if (userAddress is null || userAddress.UserId != command.UserId)
            throw new KeyNotFoundException($"Address with ID {command.AddressId} not found for this user");

        var address = await _addressRepository.GetByIdAsync(userAddress.AddressId, cancellationToken);
        if (address is null)
            throw new KeyNotFoundException($"Address with ID {userAddress.AddressId} not found");

        address.City = command.City;
        address.Street = command.Street;
        address.Number = command.Number;
        address.PostalCode = command.PostalCode;
        address.Latitude = command.Latitude;
        address.Longitude = command.Longitude;

        await _addressRepository.UpdateAsync(address, cancellationToken);
        await _addressRepository.SaveChangesAsync(cancellationToken);

        return new UpdateUserAddressResult
        {
            Id = userAddress.Id,
            UserId = userAddress.UserId,
            AddressId = address.Id,
            City = address.City,
            Street = address.Street,
            Number = address.Number,
            PostalCode = address.PostalCode,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            IsDefault = userAddress.IsDefault,
            IsActive = userAddress.IsActive
        };
    }
}
