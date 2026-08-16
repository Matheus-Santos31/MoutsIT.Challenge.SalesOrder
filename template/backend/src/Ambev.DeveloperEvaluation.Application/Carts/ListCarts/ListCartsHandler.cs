using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsHandler : IRequestHandler<ListCartsCommand, ListCartsResult>
{
    private readonly ICartRepository _cartRepository;

    public ListCartsHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<ListCartsResult> Handle(ListCartsCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListCartsValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var filters = new List<Expression<Func<Cart, bool>>>();

        if (!command.IsRequestingUserAdmin)
            filters.Add(x => x.UserId == command.RequestingUserId);

        if (command.BranchId.HasValue)
            filters.Add(x => x.BranchId == command.BranchId.Value);

        if (command.Status.HasValue)
            filters.Add(x => x.Status == command.Status.Value);

        var (carts, totalCount) = await _cartRepository.GetPagedAsync(command.Page, command.PageSize, filters, cancellationToken: cancellationToken);

        var items = carts.Select(x => new CartListItemResult
        {
            Id = x.Id,
            BranchId = x.BranchId,
            UserId = x.UserId,
            Status = x.Status,
            TotalItems = x.TotalItems,
            TotalAmount = x.TotalAmount
        });

        return new ListCartsResult { Items = items, TotalCount = totalCount };
    }
}
