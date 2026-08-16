using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetCartHandler"/> class, covering the rule that a
/// cart still open for shopping shows live catalog pricing instead of a price reserved at
/// the moment the item was added (no price reservation before checkout).
/// </summary>
public class GetCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly GetCartHandler _handler;

    public GetCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new GetCartHandler(_cartRepository);
    }

    [Fact(DisplayName = "Given a product whose price changed after being added When reading the cart Then shows the current price, not the stale one")]
    public async Task Handle_ProductPriceChangedSinceItemWasAdded_ReflectsCurrentPrice()
    {
        // Given
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer", Price = 20m };
        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            // Stale values persisted back when the item was added at the old price (10m).
            Discount = 0m,
            TotalAmount = 20m
        };
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, Status = CartStatus.Active, Items = [item] };

        _cartRepository.GetByIdWithItemsAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);

        var command = new GetCartCommand { Id = cart.Id, RequestingUserId = userId };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        var resultItem = result.Items.Should().ContainSingle().Subject;
        resultItem.UnitPrice.Should().Be(20m); // current Product.Price, not the price at add time
        resultItem.TotalAmount.Should().Be(40m); // 2 * 20, no discount tier reached
        result.TotalAmount.Should().Be(40m);
    }
}
