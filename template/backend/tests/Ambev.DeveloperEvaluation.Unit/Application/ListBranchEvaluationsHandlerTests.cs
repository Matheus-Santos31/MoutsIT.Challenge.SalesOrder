using Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListBranchEvaluationsHandler"/> class.
/// </summary>
public class ListBranchEvaluationsHandlerTests
{
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;
    private readonly ListBranchEvaluationsHandler _handler;

    public ListBranchEvaluationsHandlerTests()
    {
        _branchEvaluationRepository = Substitute.For<IBranchEvaluationRepository>();
        _handler = new ListBranchEvaluationsHandler(_branchEvaluationRepository);
    }

    [Fact(DisplayName = "Given a branch with evaluations When listing Then returns the paged items")]
    public async Task Handle_BranchWithEvaluations_ReturnsPagedItems()
    {
        var branchId = Guid.NewGuid();
        var evaluation = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = branchId, UserId = Guid.NewGuid(), Rate = 5, Comment = "Great" };

        _branchEvaluationRepository.GetPagedAsync(1, 10, Arg.Any<IEnumerable<System.Linq.Expressions.Expression<Func<BranchEvaluation, bool>>>>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((new List<BranchEvaluation> { evaluation }, 1));

        var command = new ListBranchEvaluationsCommand { BranchId = branchId, Page = 1, PageSize = 10 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(x => x.Id == evaluation.Id && x.Rate == 5);
    }

    [Fact(DisplayName = "Given an empty branch id When listing Then throws ValidationException")]
    public async Task Handle_EmptyBranchId_ThrowsValidationException()
    {
        var command = new ListBranchEvaluationsCommand { BranchId = Guid.Empty, Page = 1, PageSize = 10 };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
