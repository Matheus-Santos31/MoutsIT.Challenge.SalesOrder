using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCartItem;

public class DeleteCartItemHandler : IRequestHandler<DeleteCartItemCommand, DeleteCartItemResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;

    public DeleteCartItemHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
    }

    public async Task<DeleteCartItemResponse> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {request.CartId} not found");

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only manage your own carts.");

        if (cart.Status is CartStatus.Completed or CartStatus.Cancelled)
            throw new DomainException("Cannot change items of a cart that is completed or cancelled.");

        var item = await _cartItemRepository.GetByIdAsync(request.ItemId, cancellationToken);
        if (item is null || item.CartId != request.CartId)
            throw new KeyNotFoundException($"Item with ID {request.ItemId} not found for this cart");

        await _cartItemRepository.DeleteAsync(item, cancellationToken);
        await _cartItemRepository.SaveChangesAsync(cancellationToken);

        var remainingItems = await _cartItemRepository.GetAsync(x => x.CartId == request.CartId, cancellationToken);
        cart.RecalculateTotals(remainingItems);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new DeleteCartItemResponse { Success = true };
    }
}
