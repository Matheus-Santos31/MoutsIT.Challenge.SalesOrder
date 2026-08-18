using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHandler"/> class.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new GetSaleHandler(_saleRepository);
    }

    private static Sale BuildSale(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = 1001,
        UserId = userId,
        CustomerName = "Ana Cliente",
        CustomerEmail = "ana@test.com",
        TotalAmount = 100m,
        Items = [new SaleItem { Id = Guid.NewGuid(), ProductTitle = "Beer", Quantity = 2, UnitPrice = 50m, TotalAmount = 100m }]
    };

    [Fact(DisplayName = "Given the sale owner When getting Then returns the flattened result with items")]
    public async Task Handle_Owner_ReturnsFlattenedResult()
    {
        var userId = Guid.NewGuid();
        var sale = BuildSale(userId);
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new GetSaleCommand { Id = sale.Id, RequestingUserId = userId }, CancellationToken.None);

        result.OrderId.Should().Be(1001);
        result.Items.Should().ContainSingle(x => x.ProductTitle == "Beer");
    }

    [Fact(DisplayName = "Given an Admin who does not own the sale When getting Then succeeds")]
    public async Task Handle_Admin_CanViewAnyonesSale()
    {
        var sale = BuildSale(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new GetSaleCommand { Id = sale.Id, RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = true }, CancellationToken.None);

        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given a user who does not own the sale nor is Admin When getting Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var sale = BuildSale(Guid.NewGuid());
        _saleRepository.GetByIdWithItemsAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new GetSaleCommand { Id = sale.Id, RequestingUserId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given a sale that does not exist When getting Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdWithItemsAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new GetSaleCommand { Id = saleId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty id When getting Then throws ValidationException")]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetSaleCommand { Id = Guid.Empty }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
