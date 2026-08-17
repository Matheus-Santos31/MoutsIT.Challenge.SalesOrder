using Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.UpdateBranchAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the Manager-branch-scope check shared by the branch address
/// handlers: Admin can manage any branch's address, a Manager only their assigned branch.
/// </summary>
public class CreateBranchAddressHandlerScopeTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly CreateBranchAddressHandler _handler;

    public CreateBranchAddressHandlerScopeTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _addressRepository = Substitute.For<IAddressRepository>();
        _branchAddressRepository = Substitute.For<IBranchAddressRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new CreateBranchAddressHandler(_branchRepository, _addressRepository, _branchAddressRepository, _branchManagerRepository);
    }

    private static CreateBranchAddressCommand BuildCommand(Guid branchId) => new()
    {
        BranchId = branchId,
        City = "City",
        Street = "Street",
        Number = 100,
        PostalCode = "12345-000"
    };

    [Fact(DisplayName = "Given the Manager assigned to the branch When creating its address Then succeeds")]
    public async Task Handle_AssignedManager_CreatesAddress()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchAddressRepository.GetByBranchIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns((BranchAddress?)null);

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branch.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = BuildCommand(branch.Id);
        command.RequestingUserId = managerId;

        var result = await _handler.Handle(command, CancellationToken.None);

        result.BranchId.Should().Be(branch.Id);
    }

    [Fact(DisplayName = "Given a Manager not assigned to the branch When creating its address Then throws UnauthorizedAccessException")]
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
    }
}

public class UpdateBranchAddressHandlerScopeTests
{
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly UpdateBranchAddressHandler _handler;

    public UpdateBranchAddressHandlerScopeTests()
    {
        _branchAddressRepository = Substitute.For<IBranchAddressRepository>();
        _addressRepository = Substitute.For<IAddressRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new UpdateBranchAddressHandler(_branchAddressRepository, _addressRepository, _branchManagerRepository);
    }

    private static UpdateBranchAddressCommand BuildCommand(Guid branchId) => new()
    {
        BranchId = branchId,
        City = "City",
        Street = "Street",
        Number = 100,
        PostalCode = "12345-000"
    };

    [Fact(DisplayName = "Given the Manager assigned to the branch When updating its address Then succeeds")]
    public async Task Handle_AssignedManager_UpdatesAddress()
    {
        var branchId = Guid.NewGuid();
        var address = new Address { Id = Guid.NewGuid() };
        var branchAddress = new BranchAddress { Id = Guid.NewGuid(), BranchId = branchId, AddressId = address.Id };

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branchId, Arg.Any<CancellationToken>()).Returns(true);
        _branchAddressRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branchAddress);
        _addressRepository.GetByIdAsync(address.Id, Arg.Any<CancellationToken>()).Returns(address);

        var command = BuildCommand(branchId);
        command.RequestingUserId = managerId;

        var result = await _handler.Handle(command, CancellationToken.None);

        result.BranchId.Should().Be(branchId);
        await _addressRepository.Received(1).UpdateAsync(address, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a Manager not assigned to the branch When updating its address Then throws UnauthorizedAccessException")]
    public async Task Handle_ManagerOfDifferentBranch_ThrowsUnauthorizedAccessException()
    {
        var branchId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branchId, Arg.Any<CancellationToken>()).Returns(false);

        var command = BuildCommand(branchId);
        command.RequestingUserId = managerId;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _addressRepository.DidNotReceive().UpdateAsync(Arg.Any<Address>(), Arg.Any<CancellationToken>());
    }
}

public class DeleteBranchAddressHandlerScopeTests
{
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;
    private readonly DeleteBranchAddressHandler _handler;

    public DeleteBranchAddressHandlerScopeTests()
    {
        _branchAddressRepository = Substitute.For<IBranchAddressRepository>();
        _branchManagerRepository = Substitute.For<IBranchManagerRepository>();
        _handler = new DeleteBranchAddressHandler(_branchAddressRepository, _branchManagerRepository);
    }

    [Fact(DisplayName = "Given the Manager assigned to the branch When deleting its address Then succeeds")]
    public async Task Handle_AssignedManager_DeletesAddress()
    {
        var branchId = Guid.NewGuid();
        var branchAddress = new BranchAddress { Id = Guid.NewGuid(), BranchId = branchId };

        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branchId, Arg.Any<CancellationToken>()).Returns(true);
        _branchAddressRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branchAddress);

        var command = new DeleteBranchAddressCommand(branchId) { RequestingUserId = managerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchAddressRepository.Received(1).DeleteAsync(branchAddress, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a Manager not assigned to the branch When deleting its address Then throws UnauthorizedAccessException")]
    public async Task Handle_ManagerOfDifferentBranch_ThrowsUnauthorizedAccessException()
    {
        var branchId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        _branchManagerRepository.IsManagerOfBranchAsync(managerId, branchId, Arg.Any<CancellationToken>()).Returns(false);

        var command = new DeleteBranchAddressCommand(branchId) { RequestingUserId = managerId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _branchAddressRepository.DidNotReceive().DeleteAsync(Arg.Any<BranchAddress>(), Arg.Any<CancellationToken>());
    }
}
