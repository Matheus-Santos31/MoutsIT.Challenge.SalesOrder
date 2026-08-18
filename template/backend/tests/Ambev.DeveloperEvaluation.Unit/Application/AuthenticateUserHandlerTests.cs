using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="AuthenticateUserHandler"/> class.
/// </summary>
public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact(DisplayName = "Given correct credentials for an active user When authenticating Then returns a token")]
    public async Task Handle_ValidCredentialsActiveUser_ReturnsToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@test.com", Username = "ana", Password = "hashed", Role = UserRole.Customer, Status = UserStatus.Active };
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("Test@123", user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("jwt-token");

        var result = await _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "Test@123" }, CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("Customer");
    }

    [Fact(DisplayName = "Given a user that does not exist When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userRepository.GetByEmailAsync("missing@test.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = "missing@test.com", Password = "Test@123" }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given the wrong password When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@test.com", Password = "hashed", Status = UserStatus.Active };
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("wrong", user.Password).Returns(false);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "wrong" }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given a suspended user with correct credentials When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_SuspendedUser_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "ana@test.com", Password = "hashed", Status = UserStatus.Suspended };
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("Test@123", user.Password).Returns(true);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "Test@123" }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<Ambev.DeveloperEvaluation.Common.Security.IUser>());
    }
}
