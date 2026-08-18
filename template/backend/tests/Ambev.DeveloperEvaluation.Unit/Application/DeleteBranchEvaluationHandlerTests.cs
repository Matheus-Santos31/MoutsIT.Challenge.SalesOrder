using Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchEvaluation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteBranchEvaluationHandler"/> class, covering
/// the rate recalculation and the delete-the-aggregate-when-empty rule.
/// </summary>
public class DeleteBranchEvaluationHandlerTests
{
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;
    private readonly IBranchRateRepository _branchRateRepository;
    private readonly DeleteBranchEvaluationHandler _handler;

    public DeleteBranchEvaluationHandlerTests()
    {
        _branchEvaluationRepository = Substitute.For<IBranchEvaluationRepository>();
        _branchRateRepository = Substitute.For<IBranchRateRepository>();
        _handler = new DeleteBranchEvaluationHandler(_branchEvaluationRepository, _branchRateRepository);
    }

    [Fact(DisplayName = "Given other evaluations remain When deleting one Then recalculates the rate instead of deleting it")]
    public async Task Handle_OtherEvaluationsRemain_RecalculatesRate()
    {
        var branchId = Guid.NewGuid();
        var evaluation = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = branchId, Rate = 2 };
        var remaining = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = branchId, Rate = 4 };
        var rate = new BranchRate { BranchId = branchId, AverageRate = 3, ReviewCount = 2 };

        _branchEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);
        _branchEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BranchEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BranchEvaluation> { remaining });
        _branchRateRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(rate);

        var result = await _handler.Handle(new DeleteBranchEvaluationCommand(branchId, evaluation.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        rate.AverageRate.Should().Be(4);
        rate.ReviewCount.Should().Be(1);
        await _branchRateRepository.Received(1).UpdateAsync(rate, Arg.Any<CancellationToken>());
        await _branchRateRepository.DidNotReceive().DeleteAsync(Arg.Any<BranchRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given no evaluations remain When deleting the last one Then deletes the rate aggregate too")]
    public async Task Handle_LastEvaluation_DeletesRateAggregate()
    {
        var branchId = Guid.NewGuid();
        var evaluation = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = branchId, Rate = 2 };
        var rate = new BranchRate { BranchId = branchId, AverageRate = 2, ReviewCount = 1 };

        _branchEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);
        _branchEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BranchEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BranchEvaluation>());
        _branchRateRepository.GetByBranchIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(rate);

        var result = await _handler.Handle(new DeleteBranchEvaluationCommand(branchId, evaluation.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        await _branchRateRepository.Received(1).DeleteAsync(rate, Arg.Any<CancellationToken>());
        await _branchRateRepository.DidNotReceive().UpdateAsync(Arg.Any<BranchRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an evaluation that does not belong to the branch When deleting Then throws KeyNotFoundException")]
    public async Task Handle_EvaluationBelongsToDifferentBranch_ThrowsKeyNotFoundException()
    {
        var evaluation = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        _branchEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);

        var act = () => _handler.Handle(new DeleteBranchEvaluationCommand(Guid.NewGuid(), evaluation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an evaluation that does not exist When deleting Then throws KeyNotFoundException")]
    public async Task Handle_EvaluationNotFound_ThrowsKeyNotFoundException()
    {
        var evaluationId = Guid.NewGuid();
        _branchEvaluationRepository.GetByIdAsync(evaluationId, Arg.Any<CancellationToken>()).Returns((BranchEvaluation?)null);

        var act = () => _handler.Handle(new DeleteBranchEvaluationCommand(Guid.NewGuid(), evaluationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
