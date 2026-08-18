using Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetBranchAddressHandler"/> class.
/// </summary>
public class GetBranchAddressHandlerTests
{
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly GetBranchAddressHandler _handler;

    public GetBranchAddressHandlerTests()
    {
        _branchAddressRepository = Substitute.For<IBranchAddressRepository>();
        _handler = new GetBranchAddressHandler(_branchAddressRepository);
    }

    [Fact(DisplayName = "Given a branch with a registered address When getting Then returns the flattened result")]
    public async Task Handle_BranchWithAddress_ReturnsFlattenedResult()
    {
        var branchId = Guid.NewGuid();
        var address = new Address { Id = Guid.NewGuid(), City = "São Paulo", Street = "Av. Paulista", Number = 1000, PostalCode = "01310-100" };
        var branchAddress = new BranchAddress { Id = Guid.NewGuid(), BranchId = branchId, AddressId = address.Id, Address = address };

        _branchAddressRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branchAddress);

        var result = await _handler.Handle(new GetBranchAddressCommand(branchId), CancellationToken.None);

        result.BranchId.Should().Be(branchId);
        result.City.Should().Be("São Paulo");
        result.Number.Should().Be(1000);
    }

    [Fact(DisplayName = "Given a branch with no registered address When getting Then throws KeyNotFoundException")]
    public async Task Handle_NoAddress_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        _branchAddressRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns((BranchAddress?)null);

        var act = () => _handler.Handle(new GetBranchAddressCommand(branchId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
