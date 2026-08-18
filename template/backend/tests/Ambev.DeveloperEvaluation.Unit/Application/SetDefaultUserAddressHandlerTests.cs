using Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="SetDefaultUserAddressHandler"/> class, covering the
/// single-default-address-per-user rule.
/// </summary>
public class SetDefaultUserAddressHandlerTests
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly SetDefaultUserAddressHandler _handler;

    public SetDefaultUserAddressHandlerTests()
    {
        _userAddressRepository = Substitute.For<IUserAddressRepository>();
        _handler = new SetDefaultUserAddressHandler(_userAddressRepository);
    }

    [Fact(DisplayName = "Given another address is currently default When setting a new one Then unsets the old and sets the new")]
    public async Task Handle_AnotherAddressIsDefault_SwapsTheDefault()
    {
        var userId = Guid.NewGuid();
        var oldDefault = new UserAddress { Id = Guid.NewGuid(), UserId = userId, IsDefault = true };
        var target = new UserAddress { Id = Guid.NewGuid(), UserId = userId, IsDefault = false };

        _userAddressRepository.GetByIdAsync(target.Id, Arg.Any<CancellationToken>()).Returns(target);
        _userAddressRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<UserAddress> { oldDefault, target });

        var command = new SetDefaultUserAddressCommand { UserId = userId, AddressId = target.Id, RequestingUserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsDefault.Should().BeTrue();
        oldDefault.IsDefault.Should().BeFalse();
        target.IsDefault.Should().BeTrue();
        await _userAddressRepository.Received(1).UpdateAsync(oldDefault, Arg.Any<CancellationToken>());
        await _userAddressRepository.Received(1).UpdateAsync(target, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When setting default Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = new SetDefaultUserAddressCommand { UserId = Guid.NewGuid(), AddressId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an address that belongs to a different user When setting default Then throws KeyNotFoundException")]
    public async Task Handle_AddressBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var target = new UserAddress { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _userAddressRepository.GetByIdAsync(target.Id, Arg.Any<CancellationToken>()).Returns(target);

        var command = new SetDefaultUserAddressCommand { UserId = userId, AddressId = target.Id, RequestingUserId = userId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty address id When setting default Then throws ValidationException")]
    public async Task Handle_EmptyAddressId_ThrowsValidationException()
    {
        var command = new SetDefaultUserAddressCommand { UserId = Guid.NewGuid(), AddressId = Guid.Empty, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
