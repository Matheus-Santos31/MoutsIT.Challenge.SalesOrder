using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCartItem;

public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, CartItemResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;

    public UpdateCartItemHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }

    public async Task<CartItemResult> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateCartItemValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {command.CartId} not found");

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only manage your own carts.");

        if (cart.Status is CartStatus.Completed or CartStatus.Cancelled)
            throw new DomainException("Cannot change items of a cart that is completed or cancelled.");

        var item = await _cartItemRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null || item.CartId != command.CartId)
            throw new KeyNotFoundException($"Item with ID {command.ItemId} not found for this cart");

        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");

        item.ApplyPricing(product.Price, command.Quantity);
        await _cartItemRepository.UpdateAsync(item, cancellationToken);
        await _cartItemRepository.SaveChangesAsync(cancellationToken);

        var allItems = await _cartItemRepository.GetAsync(x => x.CartId == command.CartId, cancellationToken);
        cart.RecalculateTotals(allItems);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new CartItemResult
        {
            Id = item.Id,
            CartId = item.CartId,
            ProductId = item.ProductId,
            ProductTitle = product.Title,
            Quantity = item.Quantity,
            UnitPrice = product.Price,
            Discount = item.Discount,
            TotalAmount = item.TotalAmount,
            CartStatus = cart.Status,
            CartTotalItems = cart.TotalItems,
            CartTotalAmount = cart.TotalAmount
        };
    }
}
