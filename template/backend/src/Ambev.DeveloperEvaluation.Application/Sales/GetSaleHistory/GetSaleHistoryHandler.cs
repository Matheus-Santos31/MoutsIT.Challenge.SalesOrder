using Ambev.DeveloperEvaluation.Common.ReadModels;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleHistory;

/// <summary>
/// Reads the sale history read model directly — no join against Postgres. Eventually
/// consistent: populated by the OutboxProcessor after each Sale-related event is dispatched.
/// </summary>
public class GetSaleHistoryHandler : IRequestHandler<GetSaleHistoryCommand, IEnumerable<GetSaleHistoryResult>>
{
    private readonly ISalesReadModelStore _readModelStore;

    public GetSaleHistoryHandler(ISalesReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task<IEnumerable<GetSaleHistoryResult>> Handle(GetSaleHistoryCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetSaleHistoryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != request.UserId)
            throw new UnauthorizedAccessException("You can only view your own sale history.");

        try
        {
            var documents = await _readModelStore.GetByUserIdAsync(request.UserId, cancellationToken);

            return documents.Select(x => new GetSaleHistoryResult
            {
                SaleId = x.SaleId,
                OrderId = x.OrderId,
                CustomerName = x.CustomerName,
                BranchName = x.BranchName,
                TotalAmount = x.TotalAmount,
                ProductsQuantity = x.ProductsQuantity,
                ItemsQuantity = x.ItemsQuantity,
                TotalDiscount = x.TotalDiscount,
                Status = x.Status,
                Items = x.Items.Select(item => new GetSaleHistoryItemResult
                {
                    ProductId = item.ProductId,
                    ProductTitle = item.ProductTitle,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TotalAmount = item.TotalAmount,
                    Status = item.Status
                }),
                CreatedAt = x.CreatedAt
            });

        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
