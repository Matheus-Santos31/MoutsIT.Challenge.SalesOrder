using Ambev.DeveloperEvaluation.Application.Branches.ListBranches;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListBranchesHandler"/> class, covering pagination
/// and the merge of each branch with its denormalized rate.
/// </summary>
public class ListBranchesHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchRateRepository _branchRateRepository;
    private readonly ListBranchesHandler _handler;

    public ListBranchesHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchRateRepository = Substitute.For<IBranchRateRepository>();
        _handler = new ListBranchesHandler(_branchRepository, _branchRateRepository);
    }

    [Fact(DisplayName = "Given branches with rates When listing Then merges each rate by branch id")]
    public async Task Handle_BranchesWithRates_MergesRatesByBranchId()
    {
        var branchA = new Branch { Id = Guid.NewGuid(), Name = "A" };
        var branchB = new Branch { Id = Guid.NewGuid(), Name = "B" };

        _branchRepository.GetPagedAsync(1, 10, Arg.Any<IEnumerable<System.Linq.Expressions.Expression<Func<Branch, bool>>>>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((new List<Branch> { branchA, branchB }, 2));

        _branchRateRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BranchRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BranchRate> { new() { BranchId = branchA.Id, AverageRate = 3m, ReviewCount = 2 } });

        var result = await _handler.Handle(new ListBranchesCommand { Page = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(x => x.Id == branchA.Id && x.AverageRate == 3m);
        result.Items.Should().Contain(x => x.Id == branchB.Id && x.AverageRate == null);
    }

    [Fact(DisplayName = "Given an invalid page number When listing Then throws ValidationException")]
    public async Task Handle_InvalidPage_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new ListBranchesCommand { Page = 0, PageSize = 10 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
