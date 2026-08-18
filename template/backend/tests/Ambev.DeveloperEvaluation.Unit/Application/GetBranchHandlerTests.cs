using Ambev.DeveloperEvaluation.Application.Branches.GetBranch;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetBranchHandler"/> class, covering the merge of
/// the branch record with its denormalized rate aggregate.
/// </summary>
public class GetBranchHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchRateRepository _branchRateRepository;
    private readonly IMapper _mapper;
    private readonly GetBranchHandler _handler;

    public GetBranchHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchRateRepository = Substitute.For<IBranchRateRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetBranchHandler(_branchRepository, _branchRateRepository, _mapper);
    }

    [Fact(DisplayName = "Given a branch with an existing rate When getting Then merges rate into the result")]
    public async Task Handle_BranchWithRate_MergesRateIntoResult()
    {
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Branch" };
        var rate = new BranchRate { BranchId = branch.Id, AverageRate = 4.2m, ReviewCount = 7 };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchRateRepository.GetByBranchIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(rate);
        _mapper.Map<GetBranchResult>(branch).Returns(new GetBranchResult { Id = branch.Id, Name = branch.Name });

        var result = await _handler.Handle(new GetBranchCommand(branch.Id), CancellationToken.None);

        result.AverageRate.Should().Be(4.2m);
        result.ReviewCount.Should().Be(7);
    }

    [Fact(DisplayName = "Given a branch that does not exist When getting Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        var branchId = Guid.NewGuid();
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        var act = () => _handler.Handle(new GetBranchCommand(branchId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty id When getting Then throws ValidationException")]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetBranchCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
