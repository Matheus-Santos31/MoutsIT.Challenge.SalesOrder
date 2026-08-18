using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListCartsHandler"/> class — who gets to see which
/// carts: Admin sees everything (optionally narrowed by branch), an assigned Manager sees
/// only their branch's carts, and everyone else only sees their own.
/// </summary>
public class ListCartsHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly ListCartsHandler _handler;

    public ListCartsHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new ListCartsHandler(_cartRepository, _branchManagerRepository);
    }

    private static bool PassesAllFilters(IEnumerable<Expression<Func<Cart, bool>>>? filters, Cart cart) =>
        filters is not null && filters.All(f => f.Compile()(cart));

    [Fact(DisplayName = "Given an Admin with no branch filter When listing Then applies no ownership or branch filter")]
    public async Task Handle_Admin_NoFilters_SeesEverything()
    {
        IEnumerable<Expression<Func<Cart, bool>>>? captured = null;
        _cartRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Do<IEnumerable<Expression<Func<Cart, bool>>>?>(f => captured = f),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns((Enumerable.Empty<Cart>(), 0));

        var command = new ListCartsCommand { IsRequestingUserAdmin = true, RequestingUserId = Guid.NewGuid() };

        await _handler.Handle(command, CancellationToken.None);

        var someoneElsesCart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        PassesAllFilters(captured, someoneElsesCart).Should().BeTrue();
    }

    [Fact(DisplayName = "Given an Admin filtering by branch When listing Then only that branch's carts pass")]
    public async Task Handle_Admin_WithBranchFilter_ScopesToBranch()
    {
        IEnumerable<Expression<Func<Cart, bool>>>? captured = null;
        _cartRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Do<IEnumerable<Expression<Func<Cart, bool>>>?>(f => captured = f),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns((Enumerable.Empty<Cart>(), 0));

        var branchId = Guid.NewGuid();
        var command = new ListCartsCommand { IsRequestingUserAdmin = true, BranchId = branchId };

        await _handler.Handle(command, CancellationToken.None);

        var matching = new Cart { Id = Guid.NewGuid(), BranchId = branchId };
        var other = new Cart { Id = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        PassesAllFilters(captured, matching).Should().BeTrue();
        PassesAllFilters(captured, other).Should().BeFalse();
    }

    [Fact(DisplayName = "Given a Manager assigned to a branch When listing Then only that branch's carts pass, regardless of owner")]
    public async Task Handle_AssignedManager_ScopesToOwnBranch()
    {
        IEnumerable<Expression<Func<Cart, bool>>>? captured = null;
        _cartRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Do<IEnumerable<Expression<Func<Cart, bool>>>?>(f => captured = f),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns((Enumerable.Empty<Cart>(), 0));

        var managerId = Guid.NewGuid();
        var assignedBranchId = Guid.NewGuid();
        _branchManagerRepository.GetByUserIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns(new BranchManager { BranchId = assignedBranchId, UserId = managerId });

        var command = new ListCartsCommand { IsRequestingUserManager = true, RequestingUserId = managerId };

        await _handler.Handle(command, CancellationToken.None);

        var cartAtOwnBranchFromOtherUser = new Cart { Id = Guid.NewGuid(), BranchId = assignedBranchId, UserId = Guid.NewGuid() };
        var cartAtOtherBranch = new Cart { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), UserId = managerId };
        PassesAllFilters(captured, cartAtOwnBranchFromOtherUser).Should().BeTrue();
        PassesAllFilters(captured, cartAtOtherBranch).Should().BeFalse();
    }

    [Fact(DisplayName = "Given a Manager with no branch assignment When listing Then falls back to only their own carts")]
    public async Task Handle_UnassignedManager_FallsBackToOwnCarts()
    {
        IEnumerable<Expression<Func<Cart, bool>>>? captured = null;
        _cartRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Do<IEnumerable<Expression<Func<Cart, bool>>>?>(f => captured = f),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns((Enumerable.Empty<Cart>(), 0));

        var managerId = Guid.NewGuid();
        _branchManagerRepository.GetByUserIdAsync(managerId, Arg.Any<CancellationToken>()).Returns((BranchManager?)null);

        var command = new ListCartsCommand { IsRequestingUserManager = true, RequestingUserId = managerId };

        await _handler.Handle(command, CancellationToken.None);

        var ownCart = new Cart { Id = Guid.NewGuid(), UserId = managerId, BranchId = Guid.NewGuid() };
        var someoneElsesCart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        PassesAllFilters(captured, ownCart).Should().BeTrue();
        PassesAllFilters(captured, someoneElsesCart).Should().BeFalse();
    }

    [Fact(DisplayName = "Given a Customer When listing Then only their own carts pass")]
    public async Task Handle_Customer_OnlySeesOwnCarts()
    {
        IEnumerable<Expression<Func<Cart, bool>>>? captured = null;
        _cartRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Do<IEnumerable<Expression<Func<Cart, bool>>>?>(f => captured = f),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns((Enumerable.Empty<Cart>(), 0));

        var customerId = Guid.NewGuid();
        var command = new ListCartsCommand { RequestingUserId = customerId };

        await _handler.Handle(command, CancellationToken.None);

        var ownCart = new Cart { Id = Guid.NewGuid(), UserId = customerId, BranchId = Guid.NewGuid() };
        var someoneElsesCart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        PassesAllFilters(captured, ownCart).Should().BeTrue();
        PassesAllFilters(captured, someoneElsesCart).Should().BeFalse();

        await _branchManagerRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
