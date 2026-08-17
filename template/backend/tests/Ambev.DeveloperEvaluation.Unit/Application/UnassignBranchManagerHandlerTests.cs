using Ambev.DeveloperEvaluation.Application.Branches.UnassignBranchManager;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UnassignBranchManagerHandler"/> class.
/// </summary>
public class UnassignBranchManagerHandlerTests
{
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly UnassignBranchManagerHandler _handler;

    public UnassignBranchManagerHandlerTests()
    {
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new UnassignBranchManagerHandler(_branchManagerRepository);
    }

    [Fact(DisplayName = "Given a user assigned to the branch When unassigning Then deletes the mapping")]
    public async Task Handle_AssignedManager_DeletesMapping()
    {
        var branchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mapping = new BranchManager { Id = Guid.NewGuid(), BranchId = branchId, UserId = userId };
        _branchManagerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(mapping);

        var result = await _handler.Handle(new UnassignBranchManagerCommand(branchId, userId), CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchManagerRepository.Received(1).DeleteAsync(mapping, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user with no assignment at all When unassigning Then throws KeyNotFoundException")]
    public async Task Handle_NoMapping_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _branchManagerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((BranchManager?)null);

        var act = () => _handler.Handle(new UnassignBranchManagerCommand(branchId, userId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a user assigned to a different branch When unassigning from this branch Then throws KeyNotFoundException")]
    public async Task Handle_MappedToDifferentBranch_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mapping = new BranchManager { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), UserId = userId };
        _branchManagerRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(mapping);

        var act = () => _handler.Handle(new UnassignBranchManagerCommand(branchId, userId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _branchManagerRepository.DidNotReceive().DeleteAsync(Arg.Any<BranchManager>(), Arg.Any<CancellationToken>());
    }
}
