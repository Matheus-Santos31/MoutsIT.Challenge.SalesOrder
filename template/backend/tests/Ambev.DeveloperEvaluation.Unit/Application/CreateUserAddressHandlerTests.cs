using Ambev.DeveloperEvaluation.Application.Users.CreateUserAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateUserAddressHandler"/> class, covering
/// ownership and the clear-current-default-before-setting-a-new-one rule.
/// </summary>
public class CreateUserAddressHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly CreateUserAddressHandler _handler;

    public CreateUserAddressHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _addressRepository = Substitute.For<IAddressRepository>();
        _userAddressRepository = Substitute.For<IUserAddressRepository>();
        _handler = new CreateUserAddressHandler(_userRepository, _addressRepository, _userAddressRepository);
    }

    [Fact(DisplayName = "Given the owner When creating a first address Then persists it")]
    public async Task Handle_Owner_CreatesAddress()
    {
        var user = new User { Id = Guid.NewGuid() };
        var command = UserAddressHandlerTestData.GenerateValidCreateCommand(user.Id);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.UserId.Should().Be(user.Id);
        await _addressRepository.Received(1).AddAsync(Arg.Any<Address>(), Arg.Any<CancellationToken>());
        await _userAddressRepository.Received(1).AddAsync(Arg.Any<UserAddress>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given IsDefault true and an existing default address When creating Then clears the old default")]
    public async Task Handle_NewDefault_ClearsOldDefault()
    {
        var user = new User { Id = Guid.NewGuid() };
        var command = UserAddressHandlerTestData.GenerateValidCreateCommand(user.Id, isDefault: true);
        var oldDefault = new UserAddress { Id = Guid.NewGuid(), UserId = user.Id, IsDefault = true };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userAddressRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(new List<UserAddress> { oldDefault });

        await _handler.Handle(command, CancellationToken.None);

        oldDefault.IsDefault.Should().BeFalse();
        await _userAddressRepository.Received(1).UpdateAsync(oldDefault, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When creating an address Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = UserAddressHandlerTestData.GenerateValidCreateCommand(Guid.NewGuid());
        command.RequestingUserId = Guid.NewGuid();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given a user that does not exist When creating an address Then throws KeyNotFoundException")]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = UserAddressHandlerTestData.GenerateValidCreateCommand(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty city When creating an address Then throws ValidationException")]
    public async Task Handle_EmptyCity_ThrowsValidationException()
    {
        var command = UserAddressHandlerTestData.GenerateValidCreateCommand(Guid.NewGuid());
        command.City = string.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
