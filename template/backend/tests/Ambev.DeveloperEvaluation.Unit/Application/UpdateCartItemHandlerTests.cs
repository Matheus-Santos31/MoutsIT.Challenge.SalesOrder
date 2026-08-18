using Ambev.DeveloperEvaluation.Application.Carts.UpdateCartItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateCartItemHandler"/> class, covering ownership,
/// cart-status gating, and re-pricing against the live product price on quantity change.
/// </summary>
public class UpdateCartItemHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly UpdateCartItemHandler _handler;

    public UpdateCartItemHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new UpdateCartItemHandler(_cartRepository, _cartItemRepository, _productRepository);
    }

    [Fact(DisplayName = "Given a quantity that reaches a new discount tier When updating Then re-prices against the live product price")]
    public async Task Handle_QuantityCrossesDiscountTier_RepricesAgainstLivePrice()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active };
        var product = new Product { Id = Guid.NewGuid(), Price = 20m };
        var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = product.Id, Quantity = 2, TotalAmount = 40m };
        var command = new UpdateCartItemCommand { CartId = cart.Id, ItemId = item.Id, Quantity = 12, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _cartItemRepository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _cartItemRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<CartItem> { item });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Discount.Should().Be(48m); // 20% of the 240 subtotal
        result.TotalAmount.Should().Be(192m); // 12 * 20 * 0.8
        await _cartItemRepository.Received(1).UpdateAsync(item, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a cancelled cart When updating an item Then throws DomainException")]
    public async Task Handle_CancelledCart_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Cancelled };
        var command = new UpdateCartItemCommand { CartId = cart.Id, ItemId = Guid.NewGuid(), Quantity = 1, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user who does not own the cart When updating an item Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = CartStatus.Active };
        var command = new UpdateCartItemCommand { CartId = cart.Id, ItemId = Guid.NewGuid(), Quantity = 1, RequestingUserId = Guid.NewGuid() };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an item that does not belong to the cart When updating Then throws KeyNotFoundException")]
    public async Task Handle_ItemBelongsToDifferentCart_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active };
        var item = new CartItem { Id = Guid.NewGuid(), CartId = Guid.NewGuid() };
        var command = new UpdateCartItemCommand { CartId = cart.Id, ItemId = item.Id, Quantity = 1, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _cartItemRepository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a cart that does not exist When updating an item Then throws KeyNotFoundException")]
    public async Task Handle_CartNotFound_ThrowsKeyNotFoundException()
    {
        var cartId = Guid.NewGuid();
        var command = new UpdateCartItemCommand { CartId = cartId, ItemId = Guid.NewGuid(), Quantity = 1, RequestingUserId = Guid.NewGuid() };
        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns((Cart?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given more than 20 units When updating an item Then throws ValidationException")]
    public async Task Handle_MoreThanTwentyUnits_ThrowsValidationException()
    {
        var command = new UpdateCartItemCommand { CartId = Guid.NewGuid(), ItemId = Guid.NewGuid(), Quantity = 21, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
