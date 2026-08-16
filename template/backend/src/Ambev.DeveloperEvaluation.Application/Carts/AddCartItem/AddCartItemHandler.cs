using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;

public class AddCartItemHandler : IRequestHandler<AddCartItemCommand, CartItemResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;

    public AddCartItemHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }

    public async Task<CartItemResult> Handle(AddCartItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new AddCartItemValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {command.CartId} not found");

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only manage your own carts.");

        if (cart.Status is CartStatus.Completed or CartStatus.Cancelled)
            throw new DomainException("Cannot add items to a cart that is completed or cancelled.");

        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID {command.ProductId} not found");

        var existingItem = await _cartItemRepository.GetByCartAndProductAsync(command.CartId, command.ProductId, cancellationToken);
        if (existingItem != null)
            throw new DomainException("This product is already in the cart. Use the update endpoint to change its quantity.");

        var item = new CartItem { CartId = command.CartId, ProductId = command.ProductId };
        item.ApplyPricing(product.Price, command.Quantity);

        await _cartItemRepository.AddAsync(item, cancellationToken);
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
