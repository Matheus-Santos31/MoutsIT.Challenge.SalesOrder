using Ambev.DeveloperEvaluation.Application.Carts.DeleteCartItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteCartItemHandler"/> class, covering ownership,
/// cart-status gating, and totals recalculation after removal.
/// </summary>
public class DeleteCartItemHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly DeleteCartItemHandler _handler;

    public DeleteCartItemHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _handler = new DeleteCartItemHandler(_cartRepository, _cartItemRepository);
    }

    [Fact(DisplayName = "Given an item in an active cart When deleting Then removes it and recalculates the cart totals")]
    public async Task Handle_ActiveCart_RemovesItemAndRecalculatesTotals()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active, TotalItems = 5, TotalAmount = 50m };
        var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, Quantity = 5, TotalAmount = 50m };
        var command = new DeleteCartItemCommand { CartId = cart.Id, ItemId = item.Id, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _cartItemRepository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _cartItemRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<CartItem>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        cart.TotalItems.Should().Be(0);
        cart.TotalAmount.Should().Be(0);
        await _cartItemRepository.Received(1).DeleteAsync(item, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).UpdateAsync(cart, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a completed cart When deleting an item Then throws DomainException")]
    public async Task Handle_CompletedCart_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Completed };
        var command = new DeleteCartItemCommand { CartId = cart.Id, ItemId = Guid.NewGuid(), RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user who does not own the cart When deleting an item Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = CartStatus.Active };
        var command = new DeleteCartItemCommand { CartId = cart.Id, ItemId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an item that does not belong to the cart When deleting Then throws KeyNotFoundException")]
    public async Task Handle_ItemBelongsToDifferentCart_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active };
        var item = new CartItem { Id = Guid.NewGuid(), CartId = Guid.NewGuid() };
        var command = new DeleteCartItemCommand { CartId = cart.Id, ItemId = item.Id, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _cartItemRepository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a cart that does not exist When deleting an item Then throws KeyNotFoundException")]
    public async Task Handle_CartNotFound_ThrowsKeyNotFoundException()
    {
        var cartId = Guid.NewGuid();
        var command = new DeleteCartItemCommand { CartId = cartId, ItemId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };
        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns((Cart?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
