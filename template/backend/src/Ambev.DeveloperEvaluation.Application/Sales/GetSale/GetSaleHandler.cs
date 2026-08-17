using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleHandler : IRequestHandler<GetSaleCommand, GetSaleResult>
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<GetSaleResult> Handle(GetSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        if (!request.IsRequestingUserAdmin && request.RequestingUserId != sale.UserId)
            throw new UnauthorizedAccessException("You can only view your own sales.");

        return new GetSaleResult
        {
            Id = sale.Id,
            OrderId = sale.OrderId,
            CartId = sale.CartId,
            UserId = sale.UserId,
            CustomerName = sale.CustomerName,
            CustomerEmail = sale.CustomerEmail,
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            BranchDocNumber = sale.BranchDocNumber,
            BranchCompanyName = sale.BranchCompanyName,
            TotalAmount = sale.TotalAmount,
            ProductsQuantity = sale.ProductsQuantity,
            ItemsQuantity = sale.ItemsQuantity,
            TotalDiscount = sale.TotalDiscount,
            Status = sale.Status,
            CustomerAddress = ToResult(sale.CustomerAddress),
            BranchAddress = ToResult(sale.BranchAddress),
            Items = sale.Items.Select(ToResult),
            CreatedAt = sale.CreatedAt
        };
    }

    private static SaleAddressResult ToResult(SaleAddress address) => new()
    {
        City = address.City,
        Street = address.Street,
        Number = address.Number,
        PostalCode = address.PostalCode,
        Latitude = address.Latitude,
        Longitude = address.Longitude
    };

    private static SaleItemResult ToResult(SaleItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductTitle = item.ProductTitle,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        Discount = item.Discount,
        TotalAmount = item.TotalAmount,
        Status = item.Status
    };
}
