using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public class DeleteCartHandler : IRequestHandler<DeleteCartCommand, DeleteCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public DeleteCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<DeleteCartResponse> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteCartValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only manage your own carts.");

        await _cartRepository.DeleteAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return new DeleteCartResponse { Success = true };
    }
}
