using Ambev.DeveloperEvaluation.Application.Users.DeleteUserAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteUserAddressHandler"/> class.
/// </summary>
public class DeleteUserAddressHandlerTests
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly DeleteUserAddressHandler _handler;

    public DeleteUserAddressHandlerTests()
    {
        _userAddressRepository = Substitute.For<IUserAddressRepository>();
        _handler = new DeleteUserAddressHandler(_userAddressRepository);
    }

    [Fact(DisplayName = "Given the owner When deleting their address Then removes it")]
    public async Task Handle_Owner_DeletesAddress()
    {
        var userId = Guid.NewGuid();
        var userAddress = new UserAddress { Id = Guid.NewGuid(), UserId = userId };
        _userAddressRepository.GetByIdAsync(userAddress.Id, Arg.Any<CancellationToken>()).Returns(userAddress);

        var command = new DeleteUserAddressCommand { UserId = userId, AddressId = userAddress.Id, RequestingUserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _userAddressRepository.Received(1).DeleteAsync(userAddress, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When deleting Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = new DeleteUserAddressCommand { UserId = Guid.NewGuid(), AddressId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an address that belongs to a different user When deleting Then throws KeyNotFoundException")]
    public async Task Handle_AddressBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var userAddress = new UserAddress { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _userAddressRepository.GetByIdAsync(userAddress.Id, Arg.Any<CancellationToken>()).Returns(userAddress);

        var command = new DeleteUserAddressCommand { UserId = userId, AddressId = userAddress.Id, RequestingUserId = userId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty address id When deleting Then throws ValidationException")]
    public async Task Handle_EmptyAddressId_ThrowsValidationException()
    {
        var command = new DeleteUserAddressCommand { UserId = Guid.NewGuid(), AddressId = Guid.Empty, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
