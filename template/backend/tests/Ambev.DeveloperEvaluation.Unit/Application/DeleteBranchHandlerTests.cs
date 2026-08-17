using Ambev.DeveloperEvaluation.Application.Branches.DeleteBranch;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteBranchHandler"/> class — Admin can delete
/// any branch, a Manager only the branch they're assigned to.
/// </summary>
public class DeleteBranchHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly DeleteBranchHandler _handler;

    public DeleteBranchHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new DeleteBranchHandler(_branchRepository, _branchManagerRepository);
    }

    [Fact(DisplayName = "Given an Admin When deleting any branch Then succeeds without checking assignment")]
    public async Task Handle_Admin_DeletesAnyBranch()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var command = new DeleteBranchCommand(branch.Id) { RequestingUserId = Guid.NewGuid(), IsRequestingUserAdmin = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchManagerRepository.DidNotReceive().IsManagerOfBranchAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _branchRepository.Received(1).DeleteAsync(branch, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given the Manager assigned to the branch When deleting it Then succeeds")]
    public async Task Handle_AssignedManager_DeletesBranch()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branch.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = new DeleteBranchCommand(branch.Id) { RequestingUserId = managerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchRepository.Received(1).DeleteAsync(branch, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a Manager not assigned to the branch When deleting it Then throws UnauthorizedAccessException")]
    public async Task Handle_ManagerOfDifferentBranch_ThrowsUnauthorizedAccessException()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branch.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new DeleteBranchCommand(branch.Id) { RequestingUserId = managerId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _branchRepository.DidNotReceive().DeleteAsync(Arg.Any<Branch>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a branch that does not exist When deleting Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        var command = new DeleteBranchCommand(branchId) { IsRequestingUserAdmin = true };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
