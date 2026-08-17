using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Cancels an entire sale: owner, Manager or Admin.
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleItemRepository _saleItemRepository;

    public CancelSaleHandler(ISaleRepository saleRepository, ISaleItemRepository saleItemRepository)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
    }

    public async Task<CancelSaleResponse> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        var isOwner = request.RequestingUserId == sale.UserId;
        if (!isOwner && !request.IsRequestingUserAdmin && !request.IsRequestingUserManager)
            throw new UnauthorizedAccessException("You can only cancel your own sales.");

        if (sale.Status == SaleStatus.Cancelled)
            throw new DomainException("This sale is already cancelled.");

        foreach (var item in sale.Items.Where(x => x.Status == SaleItemStatus.Active))
        {
            item.Status = SaleItemStatus.Cancelled;
            await _saleItemRepository.UpdateAsync(item, cancellationToken);
        }

        sale.Status = SaleStatus.Cancelled;
        sale.RecalculateTotals();
        sale.AddDomainEvent(new SaleCancelledEvent(sale.Id, sale.OrderId));

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return new CancelSaleResponse { Success = true };
    }
}
