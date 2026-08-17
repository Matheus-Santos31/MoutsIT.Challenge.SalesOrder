using Ambev.DeveloperEvaluation.Application.Carts.CancelCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelCartHandler"/> class — the only way to
/// leave a cart without completing it (completion happens through the future POST /sales).
/// </summary>
public class CancelCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly CancelCartHandler _handler;

    public CancelCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new CancelCartHandler(_cartRepository);
    }

    [Fact(DisplayName = "Given an active cart owned by the requester When cancelling Then sets status to Cancelled")]
    public async Task Handle_ActiveCartOwnedByRequester_SetsStatusToCancelled()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active };
        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var command = new CancelCartCommand { Id = cart.Id, RequestingUserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        cart.Status.Should().Be(CartStatus.Cancelled);
        await _cartRepository.Received(1).UpdateAsync(cart, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a completed cart When cancelling Then throws DomainException")]
    public async Task Handle_CompletedCart_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Completed };
        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var command = new CancelCartCommand { Id = cart.Id, RequestingUserId = userId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user who does not own the cart When cancelling Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = CartStatus.Active };
        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var command = new CancelCartCommand { Id = cart.Id, RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = false };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
