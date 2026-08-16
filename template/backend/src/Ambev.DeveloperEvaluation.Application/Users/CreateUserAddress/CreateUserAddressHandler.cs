using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUserAddress;

public class CreateUserAddressHandler : IRequestHandler<CreateUserAddressCommand, CreateUserAddressResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUserAddressRepository _userAddressRepository;

    public CreateUserAddressHandler(
        IUserRepository userRepository,
        IAddressRepository addressRepository,
        IUserAddressRepository userAddressRepository)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _userAddressRepository = userAddressRepository;
    }

    public async Task<CreateUserAddressResult> Handle(CreateUserAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateUserAddressCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != command.UserId)
            throw new UnauthorizedAccessException("You can only manage your own addresses.");

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException($"User with ID {command.UserId} not found");

        if (command.IsDefault)
            await ClearCurrentDefaultAsync(command.UserId, cancellationToken);

        var address = new Address
        {
            City = command.City,
            Street = command.Street,
            Number = command.Number,
            PostalCode = command.PostalCode,
            Latitude = command.Latitude,
            Longitude = command.Longitude
        };

        var userAddress = new UserAddress
        {
            UserId = command.UserId,
            Address = address,
            IsDefault = command.IsDefault,
            IsActive = true
        };

        await _addressRepository.AddAsync(address, cancellationToken);
        await _userAddressRepository.AddAsync(userAddress, cancellationToken);
        await _userAddressRepository.SaveChangesAsync(cancellationToken);

        return new CreateUserAddressResult
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

    private async Task ClearCurrentDefaultAsync(Guid userId, CancellationToken cancellationToken)
    {
        var current = await _userAddressRepository.GetByUserIdAsync(userId, cancellationToken);
        foreach (var existing in current.Where(x => x.IsDefault))
        {
            existing.IsDefault = false;
            await _userAddressRepository.UpdateAsync(existing, cancellationToken);
        }
    }
}
