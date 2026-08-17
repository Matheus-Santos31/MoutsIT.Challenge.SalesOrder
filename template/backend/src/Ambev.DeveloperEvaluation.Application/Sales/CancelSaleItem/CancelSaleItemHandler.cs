using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Cancels a single line item of a sale — an operational/fulfillment action restricted to
/// Manager/Admin (enforced at the controller via role authorization, not ownership: a
/// customer never cancels their own line items directly). Recomputes the sale's rollups
/// from the remaining Active items, and auto-cancels the whole sale if none are left.
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleItemRepository _saleItemRepository;

    public CancelSaleItemHandler(ISaleRepository saleRepository, ISaleItemRepository saleItemRepository)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
    }

    public async Task<CancelSaleItemResponse> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdWithItemsAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        if (sale.Status == SaleStatus.Cancelled)
            throw new DomainException("This sale is already cancelled.");

        var item = sale.Items.FirstOrDefault(x => x.Id == request.ItemId);
        if (item is null)
            throw new KeyNotFoundException($"Item with ID {request.ItemId} not found for this sale");

        if (item.Status == SaleItemStatus.Cancelled)
            throw new DomainException("This item is already cancelled.");

        item.Status = SaleItemStatus.Cancelled;
        await _saleItemRepository.UpdateAsync(item, cancellationToken);

        sale.RecalculateTotals();

        var saleWasCancelled = sale.Items.All(x => x.Status == SaleItemStatus.Cancelled);
        if (saleWasCancelled)
            sale.Status = SaleStatus.Cancelled;

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return new CancelSaleItemResponse { Success = true, SaleWasCancelled = saleWasCancelled };
    }
}
