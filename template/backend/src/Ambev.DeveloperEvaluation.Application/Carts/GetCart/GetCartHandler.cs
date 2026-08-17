using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

/// <summary>
/// Reads a cart still open for shopping. Item pricing is recalculated against the current
/// Product.Price on every read (no price reservation before checkout, matching common
/// marketplace behavior) instead of trusting whatever was last persisted on the CartItem.
/// </summary>
public class GetCartHandler : IRequestHandler<GetCartCommand, GetCartResult>
{
    private readonly ICartRepository _cartRepository;

    public GetCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<GetCartResult> Handle(GetCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetCartValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only view your own carts.");

        var items = cart.Items.Select(x => BuildItemDetail(x)).ToList();

        return new GetCartResult
        {
            Id = cart.Id,
            BranchId = cart.BranchId,
            UserId = cart.UserId,
            Status = cart.Status,
            TotalItems = items.Sum(x => x.Quantity),
            TotalAmount = items.Sum(x => x.TotalAmount),
            Items = items
        };
    }

    private static CartItemDetail BuildItemDetail(CartItem item)
    {
        var unitPrice = item.Product?.Price ?? 0;
        var (discount, totalAmount) = QuantityDiscountPolicy.Calculate(unitPrice, item.Quantity);

        return new CartItemDetail
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductTitle = item.Product?.Title ?? string.Empty,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            Discount = discount,
            TotalAmount = totalAmount
        };
    }
}
