using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CancelCart;

/// <summary>
/// Cancels a cart still open for shopping. This is the only way to leave a cart
/// without completing it — the completed path goes through POST /sales instead,
/// which is what actually promotes a Cart to Completed and creates the Sale.
/// </summary>
public class CancelCartHandler : IRequestHandler<CancelCartCommand, CancelCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public CancelCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CancelCartResponse> Handle(CancelCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelCartValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only manage your own carts.");

        if (cart.Status == CartStatus.Completed)
            throw new DomainException("Cannot cancel a cart that has already been completed.");

        if (cart.Status == CartStatus.Cancelled)
            throw new DomainException("This cart is already cancelled.");

        cart.Status = CartStatus.Cancelled;
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new CancelCartResponse { Success = true };
    }
}
