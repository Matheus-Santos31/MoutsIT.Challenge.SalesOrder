using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Completes an active cart into a sale and promotes the cart to Completed.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IUserAddressRepository _userAddressRepository;

    public CreateSaleHandler(
        ICartRepository cartRepository,
        ISaleRepository saleRepository,
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IBranchAddressRepository branchAddressRepository,
        IUserAddressRepository userAddressRepository)
    {
        _cartRepository = cartRepository;
        _saleRepository = saleRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _branchAddressRepository = branchAddressRepository;
        _userAddressRepository = userAddressRepository;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await _cartRepository.GetByIdWithItemsAsync(command.CartId, cancellationToken);
        if (cart is null)
            throw new KeyNotFoundException($"Cart with ID {command.CartId} not found");

        if (!command.IsRequestingUserAdmin && command.RequestingUserId != cart.UserId)
            throw new UnauthorizedAccessException("You can only complete your own carts.");

        if (cart.Status != CartStatus.Active)
            throw new DomainException("Only an active cart can be completed into a sale.");

        if (cart.Items.Count == 0)
            throw new DomainException("Cannot complete an empty cart.");

        if (await _saleRepository.GetByCartIdAsync(cart.Id, cancellationToken) is not null)
            throw new DomainException("This cart has already been completed into a sale.");

        var branchAddress = await _branchAddressRepository.GetByBranchIdAsync(cart.BranchId, cancellationToken);
        if (branchAddress?.Address is null)
            throw new DomainException("The branch has no address registered. Cannot complete the cart.");

        var userAddresses = (await _userAddressRepository.GetByUserIdAsync(cart.UserId, cancellationToken)).ToList();
        var customerAddress = userAddresses.FirstOrDefault(x => x.IsDefault && x.IsActive)
            ?? userAddresses.FirstOrDefault(x => x.IsActive);
        if (customerAddress?.Address is null)
            throw new DomainException("The user has no address registered. Cannot complete the cart.");

        var branch = await _branchRepository.GetByIdAsync(cart.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch with ID {cart.BranchId} not found");
        var user = await _userRepository.GetByIdAsync(cart.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {cart.UserId} not found");

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            UserId = cart.UserId,
            BranchId = cart.BranchId,
            CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
            CustomerEmail = user.Email,
            BranchName = branch.Name,
            BranchDocNumber = branch.DocNumber,
            BranchCompanyName = branch.CompanyName,
            CustomerAddress = SaleAddress.From(customerAddress.Address),
            BranchAddress = SaleAddress.From(branchAddress.Address),
            Status = SaleStatus.Created
        };

        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Product is null)
                continue;

            var saleItem = new SaleItem { Sale = sale };
            saleItem.ApplyPricing(cartItem.Product, cartItem.Quantity);
            sale.Items.Add(saleItem);
        }

        sale.ProductsQuantity = sale.Items.Count;
        sale.ItemsQuantity = sale.Items.Sum(x => x.Quantity);
        sale.TotalDiscount = sale.Items.Sum(x => x.Discount);
        sale.TotalAmount = sale.Items.Sum(x => x.TotalAmount);

        cart.Status = CartStatus.Completed;
        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id, sale.UserId, sale.BranchId, sale.TotalAmount));

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return new CreateSaleResult
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
            Items = sale.Items.Select(ToResult)
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
