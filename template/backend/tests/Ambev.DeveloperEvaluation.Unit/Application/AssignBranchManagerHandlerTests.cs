using Ambev.DeveloperEvaluation.Application.Branches.AssignBranchManager;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="AssignBranchManagerHandler"/> class — assigning a
/// Manager-role user to a branch, upserting the single BranchManager row per user.
/// </summary>
public class AssignBranchManagerHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly AssignBranchManagerHandler _handler;

    public AssignBranchManagerHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new AssignBranchManagerHandler(_branchRepository, _userRepository, _branchManagerRepository);
    }

    [Fact(DisplayName = "Given a Manager-role user with no prior assignment When assigning Then creates a new BranchManager row")]
    public async Task Handle_UnassignedManager_CreatesMapping()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        var user = new User { Id = Guid.NewGuid(), Role = UserRole.Manager };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _branchManagerRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((BranchManager?)null);

        var command = new AssignBranchManagerCommand { BranchId = branch.Id, UserId = user.Id };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchManagerRepository.Received(1).AddAsync(
            Arg.Is<BranchManager>(x => x.BranchId == branch.Id && x.UserId == user.Id),
            Arg.Any<CancellationToken>());
        await _branchManagerRepository.DidNotReceive().UpdateAsync(Arg.Any<BranchManager>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a Manager already assigned to another branch When assigning to a new one Then reassigns (upsert)")]
    public async Task Handle_AlreadyAssignedManager_ReassignsBranch()
    {
        var newBranch = new Branch { Id = Guid.NewGuid() };
        var user = new User { Id = Guid.NewGuid(), Role = UserRole.Manager };
        var existingMapping = new BranchManager { Id = Guid.NewGuid(), UserId = user.Id, BranchId = Guid.NewGuid() };

        _branchRepository.GetByIdAsync(newBranch.Id, Arg.Any<CancellationToken>()).Returns(newBranch);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _branchManagerRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(existingMapping);

        var command = new AssignBranchManagerCommand { BranchId = newBranch.Id, UserId = user.Id };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        existingMapping.BranchId.Should().Be(newBranch.Id);
        await _branchManagerRepository.Received(1).UpdateAsync(existingMapping, Arg.Any<CancellationToken>());
        await _branchManagerRepository.DidNotReceive().AddAsync(Arg.Any<BranchManager>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a user without the Manager role When assigning Then throws DomainException")]
    public async Task Handle_NonManagerUser_ThrowsDomainException()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        var user = new User { Id = Guid.NewGuid(), Role = UserRole.Customer };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new AssignBranchManagerCommand { BranchId = branch.Id, UserId = user.Id };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given a branch that does not exist When assigning Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        var command = new AssignBranchManagerCommand { BranchId = branchId, UserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a user that does not exist When assigning Then throws KeyNotFoundException")]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new AssignBranchManagerCommand { BranchId = branch.Id, UserId = userId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
