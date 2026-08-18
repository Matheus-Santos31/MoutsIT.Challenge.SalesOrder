using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;
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
/// Contains unit tests for the <see cref="AddCartItemHandler"/> class, covering ownership,
/// cart-status gating, the no-duplicate-product rule, and the quantity-discount pricing.
/// </summary>
public class AddCartItemHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly AddCartItemHandler _handler;

    public AddCartItemHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new AddCartItemHandler(_cartRepository, _cartItemRepository, _productRepository);
    }

    private static Cart BuildActiveCart(Guid userId) => new() { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active };

    [Fact(DisplayName = "Given a product not yet in the cart When adding 5 units Then applies the 10% discount tier")]
    public async Task Handle_FiveUnits_Applies10PercentDiscount()
    {
        var userId = Guid.NewGuid();
        var cart = BuildActiveCart(userId);
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer", Price = 10m };
        var command = new AddCartItemCommand { CartId = cart.Id, ProductId = product.Id, Quantity = 5, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _cartItemRepository.GetByCartAndProductAsync(cart.Id, product.Id, Arg.Any<CancellationToken>()).Returns((CartItem?)null);
        _cartItemRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<CartItem>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Discount.Should().Be(5m); // 10% of the 50 subtotal
        result.TotalAmount.Should().Be(45m); // 5 * 10 * 0.9
        await _cartItemRepository.Received(1).AddAsync(Arg.Any<CartItem>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a product already in the cart When adding it again Then throws DomainException")]
    public async Task Handle_ProductAlreadyInCart_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var cart = BuildActiveCart(userId);
        var product = new Product { Id = Guid.NewGuid(), Price = 10m };
        var command = new AddCartItemCommand { CartId = cart.Id, ProductId = product.Id, Quantity = 1, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _cartItemRepository.GetByCartAndProductAsync(cart.Id, product.Id, Arg.Any<CancellationToken>())
            .Returns(new CartItem { CartId = cart.Id, ProductId = product.Id });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a cart already completed When adding an item Then throws DomainException")]
    public async Task Handle_CompletedCart_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Completed };
        var command = new AddCartItemCommand { CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a user who does not own the cart When adding an item Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        var cart = BuildActiveCart(Guid.NewGuid());
        var command = new AddCartItemCommand { CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1, RequestingUserId = Guid.NewGuid() };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given a product that does not exist When adding an item Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = BuildActiveCart(userId);
        var command = new AddCartItemCommand { CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1, RequestingUserId = userId };

        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _productRepository.GetByIdAsync(command.ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given more than 20 units When adding an item Then throws ValidationException")]
    public async Task Handle_MoreThanTwentyUnits_ThrowsValidationException()
    {
        var command = new AddCartItemCommand { CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 21, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
