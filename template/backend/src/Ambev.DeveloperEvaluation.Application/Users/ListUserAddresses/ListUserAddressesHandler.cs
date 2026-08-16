using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;

public class ListUserAddressesHandler : IRequestHandler<ListUserAddressesCommand, IEnumerable<UserAddressResult>>
{
    private readonly IUserAddressRepository _userAddressRepository;

    public ListUserAddressesHandler(IUserAddressRepository userAddressRepository)
    {
        _userAddressRepository = userAddressRepository;
    }

    public async Task<IEnumerable<UserAddressResult>> Handle(ListUserAddressesCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListUserAddressesValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != command.UserId)
            throw new UnauthorizedAccessException("You can only view your own addresses.");

        var addresses = await _userAddressRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        return addresses.Select(x => new UserAddressResult
        {
            Id = x.Id,
            UserId = x.UserId,
            AddressId = x.AddressId,
            City = x.Address!.City,
            Street = x.Address.Street,
            Number = x.Address.Number,
            PostalCode = x.Address.PostalCode,
            Latitude = x.Address.Latitude,
            Longitude = x.Address.Longitude,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive
        });
    }
}
