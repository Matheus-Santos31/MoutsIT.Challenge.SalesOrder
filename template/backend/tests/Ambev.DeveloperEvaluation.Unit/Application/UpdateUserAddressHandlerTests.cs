using Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserAddressHandler"/> class.
/// </summary>
public class UpdateUserAddressHandlerTests
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly UpdateUserAddressHandler _handler;

    public UpdateUserAddressHandlerTests()
    {
        _userAddressRepository = Substitute.For<IUserAddressRepository>();
        _addressRepository = Substitute.For<IAddressRepository>();
        _handler = new UpdateUserAddressHandler(_userAddressRepository, _addressRepository);
    }

    [Fact(DisplayName = "Given the owner When updating their address Then applies the changes")]
    public async Task Handle_Owner_UpdatesAddress()
    {
        var userId = Guid.NewGuid();
        var address = new Address { Id = Guid.NewGuid() };
        var userAddress = new UserAddress { Id = Guid.NewGuid(), UserId = userId, AddressId = address.Id };
        var command = UserAddressHandlerTestData.GenerateValidUpdateCommand(userId, userAddress.Id);

        _userAddressRepository.GetByIdAsync(userAddress.Id, Arg.Any<CancellationToken>()).Returns(userAddress);
        _addressRepository.GetByIdAsync(address.Id, Arg.Any<CancellationToken>()).Returns(address);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.City.Should().Be(command.City);
        address.City.Should().Be(command.City);
        await _addressRepository.Received(1).UpdateAsync(address, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When updating Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = UserAddressHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid(), Guid.NewGuid());
        command.RequestingUserId = Guid.NewGuid();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an address that belongs to a different user When updating Then throws KeyNotFoundException")]
    public async Task Handle_AddressBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var userAddress = new UserAddress { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var command = UserAddressHandlerTestData.GenerateValidUpdateCommand(userId, userAddress.Id);

        _userAddressRepository.GetByIdAsync(userAddress.Id, Arg.Any<CancellationToken>()).Returns(userAddress);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty city When updating Then throws ValidationException")]
    public async Task Handle_EmptyCity_ThrowsValidationException()
    {
        var command = UserAddressHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid(), Guid.NewGuid());
        command.City = string.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
