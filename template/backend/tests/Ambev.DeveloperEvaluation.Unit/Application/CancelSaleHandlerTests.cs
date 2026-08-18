using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleHandler"/> class — cancelling the
/// whole sale (owner, Manager or Admin), cascading to every Active item.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _saleItemRepository = Substitute.For<ISaleItemRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new CancelSaleHandler(_saleRepository, _saleItemRepository, _branchManagerRepository);
    }

    private static Sale BuildSaleWithTwoActiveItems(Guid userId, Guid branchId = default)
    {
        var item1 = new SaleItem { Id = Guid.NewGuid(), Quantity = 2, UnitPrice = 10m, Discount = 0m, TotalAmount = 20m, Status = SaleItemStatus.Active };
        var item2 = new SaleItem { Id = Guid.NewGuid(), Quantity = 1, UnitPrice = 5m, Discount = 0m, TotalAmount = 5m, Status = SaleItemStatus.Active };

        return new Sale
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BranchId = branchId == default ? Guid.NewGuid() : branchId,
            Status = SaleStatus.Created,
            Items = [item1, item2],
            ProductsQuantity = 2,
            ItemsQuantity = 3,
            TotalAmount = 25m,
            TotalDiscount = 0m
        };
    }

    [Fact(DisplayName = "Given the sale owner When cancelling Then cascades to every active item and zeroes the totals")]
    public async Task Handle_Owner_CancelsSaleAndCascadesToItems()
    {
        var userId = Guid.NewGuid();
        var sale = BuildSaleWithTwoActiveItems(userId);
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Items.Should().OnlyContain(x => x.Status == SaleItemStatus.Cancelled);
        sale.TotalAmount.Should().Be(0m);
        sale.ProductsQuantity.Should().Be(0);
        sale.ItemsQuantity.Should().Be(0);
        await _saleItemRepository.Received(2).UpdateAsync(Arg.Any<SaleItem>(), Arg.Any<CancellationToken>());
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "Given an Admin who does not own the sale When cancelling Then succeeds")]
    public async Task Handle_Admin_CancelsAnyonesSale()
    {
        var sale = BuildSaleWithTwoActiveItems(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact(DisplayName = "Given the Manager assigned to the sale's branch When cancelling Then succeeds")]
    public async Task Handle_AssignedManager_CancelsAnyonesSale()
    {
        var sale = BuildSaleWithTwoActiveItems(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, sale.BranchId, Arg.Any<CancellationToken>()).Returns(true);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = managerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact(DisplayName = "Given a Manager who is not assigned to the sale's branch When cancelling Then throws UnauthorizedAccessException")]
    public async Task Handle_ManagerOfDifferentBranch_ThrowsUnauthorizedAccessException()
    {
        var sale = BuildSaleWithTwoActiveItems(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, sale.BranchId, Arg.Any<CancellationToken>()).Returns(false);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = managerId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given a user who is neither owner, Manager nor Admin When cancelling Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonManagerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var sale = BuildSaleWithTwoActiveItems(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an already cancelled sale When cancelling again Then throws DomainException")]
    public async Task Handle_AlreadyCancelledSale_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var sale = BuildSaleWithTwoActiveItems(userId);
        sale.Status = SaleStatus.Cancelled;
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleCommand { Id = sale.Id, RequestingUserId = userId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a sale that does not exist When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdWithItemsAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var command = new CancelSaleCommand { Id = saleId, RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
