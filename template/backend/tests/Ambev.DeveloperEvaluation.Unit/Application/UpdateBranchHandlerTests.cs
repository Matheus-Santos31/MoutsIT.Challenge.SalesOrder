using Ambev.DeveloperEvaluation.Application.Branches.UpdateBranch;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBranchHandler"/> class — Admin can manage
/// any branch, a Manager only the branch they're assigned to.
/// </summary>
public class UpdateBranchHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly IMapper _mapper;
    private readonly UpdateBranchHandler _handler;

    public UpdateBranchHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateBranchHandler(_branchRepository, _branchManagerRepository, _mapper);
    }

    private static UpdateBranchCommand BuildCommand(Guid branchId) => new()
    {
        Id = branchId,
        Name = "Updated Branch",
        DocNumber = "12345678000199",
        CompanyName = "Updated Company"
    };

    [Fact(DisplayName = "Given an Admin When updating any branch Then succeeds without checking assignment")]
    public async Task Handle_Admin_UpdatesAnyBranch()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchRepository.GetByDocNumberAsync(Arg.Any<string>(), branch.Id, Arg.Any<CancellationToken>()).Returns((Branch?)null);
        _mapper.Map<UpdateBranchResult>(branch).Returns(new UpdateBranchResult());

        var command = BuildCommand(branch.Id);
        command.RequestingUserId = Guid.NewGuid();
        command.IsRequestingUserAdmin = true;

        await _handler.Handle(command, CancellationToken.None);

        await _branchManagerRepository.DidNotReceive().IsManagerOfBranchAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _branchRepository.Received(1).UpdateAsync(branch, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given the Manager assigned to the branch When updating it Then succeeds")]
    public async Task Handle_AssignedManager_UpdatesBranch()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchRepository.GetByDocNumberAsync(Arg.Any<string>(), branch.Id, Arg.Any<CancellationToken>()).Returns((Branch?)null);
        _mapper.Map<UpdateBranchResult>(branch).Returns(new UpdateBranchResult());

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branch.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommand(branch.Id);
        command.RequestingUserId = managerId;

        await _handler.Handle(command, CancellationToken.None);

        await _branchRepository.Received(1).UpdateAsync(branch, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a Manager not assigned to the branch When updating it Then throws UnauthorizedAccessException")]
    public async Task Handle_ManagerOfDifferentBranch_ThrowsUnauthorizedAccessException()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branch.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = BuildCommand(branch.Id);
        command.RequestingUserId = managerId;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _branchRepository.DidNotReceive().UpdateAsync(Arg.Any<Branch>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a branch that does not exist When updating Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        var command = BuildCommand(branchId);
        command.IsRequestingUserAdmin = true;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
