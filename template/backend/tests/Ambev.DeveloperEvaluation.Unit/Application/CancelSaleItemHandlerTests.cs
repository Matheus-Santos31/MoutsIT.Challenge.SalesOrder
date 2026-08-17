using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleItemHandler"/> class — cancelling a
/// single line item (Manager/Admin only, enforced at the controller), recomputing the
/// sale's rollups from the remaining Active items, and auto-cancelling the sale when
/// no Active items are left.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _saleItemRepository = Substitute.For<ISaleItemRepository>();
        _handler = new CancelSaleItemHandler(_saleRepository, _saleItemRepository);
    }

    private static (Sale Sale, SaleItem FirstItem, SaleItem SecondItem) BuildSaleWithTwoActiveItems()
    {
        var item1 = new SaleItem { Id = Guid.NewGuid(), Quantity = 2, UnitPrice = 10m, Discount = 0m, TotalAmount = 20m, Status = SaleItemStatus.Active };
        var item2 = new SaleItem { Id = Guid.NewGuid(), Quantity = 1, UnitPrice = 5m, Discount = 0m, TotalAmount = 5m, Status = SaleItemStatus.Active };

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Status = SaleStatus.Created,
            Items = [item1, item2],
            ProductsQuantity = 2,
            ItemsQuantity = 3,
            TotalAmount = 25m,
            TotalDiscount = 0m
        };

        return (sale, item1, item2);
    }

    [Fact(DisplayName = "Given a sale with two active items When cancelling one Then shrinks the totals and keeps the sale open")]
    public async Task Handle_OneOfTwoActiveItems_ShrinksTotalsAndKeepsSaleOpen()
    {
        var (sale, item1, item2) = BuildSaleWithTwoActiveItems();
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id, item2.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SaleWasCancelled.Should().BeFalse();
        item2.Status.Should().Be(SaleItemStatus.Cancelled);
        item1.Status.Should().Be(SaleItemStatus.Active);
        sale.Status.Should().Be(SaleStatus.Created);
        sale.TotalAmount.Should().Be(20m); // only item1 remains active
        sale.ItemsQuantity.Should().Be(2);
        sale.ProductsQuantity.Should().Be(1);
        await _saleItemRepository.Received(1).UpdateAsync(item2, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a sale with a single active item When cancelling it Then auto-cancels the whole sale")]
    public async Task Handle_LastActiveItem_AutoCancelsSale()
    {
        var (sale, item1, item2) = BuildSaleWithTwoActiveItems();
        item1.Status = SaleItemStatus.Cancelled; // already cancelled, only item2 remains active
        sale.ProductsQuantity = 1;
        sale.ItemsQuantity = 1;
        sale.TotalAmount = 5m;
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id, item2.Id), CancellationToken.None);

        result.SaleWasCancelled.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.TotalAmount.Should().Be(0m);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an already cancelled item When cancelling again Then throws DomainException")]
    public async Task Handle_AlreadyCancelledItem_ThrowsDomainException()
    {
        var (sale, item1, _) = BuildSaleWithTwoActiveItems();
        item1.Status = SaleItemStatus.Cancelled;
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, item1.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given an already cancelled sale When cancelling one of its items Then throws DomainException")]
    public async Task Handle_SaleAlreadyCancelled_ThrowsDomainException()
    {
        var (sale, item1, _) = BuildSaleWithTwoActiveItems();
        sale.Status = SaleStatus.Cancelled;
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, item1.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given an item that does not belong to the sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_ItemNotFoundInSale_ThrowsKeyNotFoundException()
    {
        var (sale, _, _) = BuildSaleWithTwoActiveItems();
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a sale that does not exist When cancelling an item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdWithItemsAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleItemCommand(saleId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
