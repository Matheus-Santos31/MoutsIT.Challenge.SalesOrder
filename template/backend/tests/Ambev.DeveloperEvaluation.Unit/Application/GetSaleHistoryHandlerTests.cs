using Ambev.DeveloperEvaluation.Application.Sales.GetSaleHistory;
using Ambev.DeveloperEvaluation.Common.ReadModels;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHistoryHandler"/> class — the read side of
/// the sale history read model (MongoDB), independent from the Postgres system of record.
/// </summary>
public class GetSaleHistoryHandlerTests
{
    private readonly ISalesReadModelStore _readModelStore;
    private readonly GetSaleHistoryHandler _handler;

    public GetSaleHistoryHandlerTests()
    {
        _readModelStore = Substitute.For<ISalesReadModelStore>();
        _handler = new GetSaleHistoryHandler(_readModelStore);
    }

    [Fact(DisplayName = "Given the owner When reading their sale history Then returns the flattened documents")]
    public async Task Handle_Owner_ReturnsFlattenedDocuments()
    {
        var userId = Guid.NewGuid();
        var document = new SaleHistoryDocument
        {
            SaleId = Guid.NewGuid(),
            OrderId = 1001,
            UserId = userId,
            CustomerName = "Ana Cliente",
            BranchName = "Filial Centro",
            TotalAmount = 100m,
            Status = "Created",
            Items = [new SaleHistoryItemDocument { ProductId = Guid.NewGuid(), ProductTitle = "Beer", Quantity = 2, UnitPrice = 50m, TotalAmount = 100m, Status = "Active" }]
        };

        _readModelStore.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns([document]);

        var result = await _handler.Handle(new GetSaleHistoryCommand { UserId = userId, RequestingUserId = userId }, CancellationToken.None);

        result.Should().ContainSingle(x => x.OrderId == 1001 && x.Items.Any(i => i.ProductTitle == "Beer"));
    }

    [Fact(DisplayName = "Given an Admin who is not the owner When reading sale history Then succeeds")]
    public async Task Handle_Admin_CanReadAnyonesHistory()
    {
        var userId = Guid.NewGuid();
        _readModelStore.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetSaleHistoryCommand { UserId = userId, RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = true }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given a user who is not the owner nor Admin When reading sale history Then throws UnauthorizedAccessException")]
    public async Task Handle_NonOwnerNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var command = new GetSaleHistoryCommand { UserId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given an empty user id When reading sale history Then throws ValidationException")]
    public async Task Handle_EmptyUserId_ThrowsValidationException()
    {
        var command = new GetSaleHistoryCommand { UserId = Guid.Empty };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
