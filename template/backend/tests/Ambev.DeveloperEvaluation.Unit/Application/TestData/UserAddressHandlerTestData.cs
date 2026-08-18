using Ambev.DeveloperEvaluation.Application.Users.CreateUserAddress;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides Bogus-based generators for UserAddress-related handler commands.
/// </summary>
public static class UserAddressHandlerTestData
{
    private static readonly Faker<CreateUserAddressCommand> createUserAddressFaker = new Faker<CreateUserAddressCommand>()
        .RuleFor(x => x.City, f => f.Address.City())
        .RuleFor(x => x.Street, f => f.Address.StreetAddress())
        .RuleFor(x => x.Number, f => f.Random.Number(1, 9999))
        .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
        .RuleFor(x => x.Latitude, f => f.Address.Latitude().ToString())
        .RuleFor(x => x.Longitude, f => f.Address.Longitude().ToString());

    private static readonly Faker<UpdateUserAddressCommand> updateUserAddressFaker = new Faker<UpdateUserAddressCommand>()
        .RuleFor(x => x.City, f => f.Address.City())
        .RuleFor(x => x.Street, f => f.Address.StreetAddress())
        .RuleFor(x => x.Number, f => f.Random.Number(1, 9999))
        .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
        .RuleFor(x => x.Latitude, f => f.Address.Latitude().ToString())
        .RuleFor(x => x.Longitude, f => f.Address.Longitude().ToString());

    public static CreateUserAddressCommand GenerateValidCreateCommand(Guid userId, bool isDefault = false)
    {
        var command = createUserAddressFaker.Generate();
        command.UserId = userId;
        command.RequestingUserId = userId;
        command.IsDefault = isDefault;
        return command;
    }

    public static UpdateUserAddressCommand GenerateValidUpdateCommand(Guid userId, Guid addressId)
    {
        var command = updateUserAddressFaker.Generate();
        command.UserId = userId;
        command.AddressId = addressId;
        command.RequestingUserId = userId;
        return command;
    }
}
