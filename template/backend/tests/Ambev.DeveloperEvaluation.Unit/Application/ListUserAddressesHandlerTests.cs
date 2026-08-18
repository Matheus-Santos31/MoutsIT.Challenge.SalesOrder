using Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListUserAddressesHandler"/> class.
/// </summary>
public class ListUserAddressesHandlerTests
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly ListUserAddressesHandler _handler;

    public ListUserAddressesHandlerTests()
    {
        _userAddressRepository = Substitute.For<IUserAddressRepository>();
        _handler = new ListUserAddressesHandler(_userAddressRepository);
    }

    [Fact(DisplayName = "Given the owner When listing their addresses Then returns the flattened results")]
    public async Task Handle_Owner_ReturnsFlattenedResults()
    {
        var userId = Guid.NewGuid();
        var address = new Address { Id = Guid.NewGuid(), City = "Curitiba" };
        var userAddress = new UserAddress { Id = Guid.NewGuid(), UserId = userId, AddressId = address.Id, Address = address, IsDefault = true };

        _userAddressRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<UserAddress> { userAddress });

        var command = new ListUserAddressesCommand { UserId = userId, RequestingUserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().ContainSingle(x => x.City == "Curitiba" && x.IsDefault);
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When listing Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = new ListUserAddressesCommand { UserId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an empty user id When listing Then throws ValidationException")]
    public async Task Handle_EmptyUserId_ThrowsValidationException()
    {
        var command = new ListUserAddressesCommand { UserId = Guid.Empty, RequestingUserId = Guid.Empty };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
