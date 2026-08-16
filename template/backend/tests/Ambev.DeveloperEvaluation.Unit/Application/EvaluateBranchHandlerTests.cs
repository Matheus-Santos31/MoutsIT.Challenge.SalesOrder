using Ambev.DeveloperEvaluation.Application.Branches.EvaluateBranch;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="EvaluateBranchHandler"/> class,
/// covering the upsert-into-evaluation and rating recalculation rules.
/// </summary>
public class EvaluateBranchHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchEvaluationRepository _branchEvaluationRepository;
    private readonly IBranchRateRepository _branchRateRepository;
    private readonly EvaluateBranchHandler _handler;

    public EvaluateBranchHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _branchEvaluationRepository = Substitute.For<IBranchEvaluationRepository>();
        _branchRateRepository = Substitute.For<IBranchRateRepository>();
        _handler = new EvaluateBranchHandler(_branchRepository, _branchEvaluationRepository, _branchRateRepository);
    }

    [Fact(DisplayName = "Given branch never evaluated by user When evaluating Then creates evaluation and rate")]
    public async Task Handle_FirstEvaluation_CreatesEvaluationAndRate()
    {
        // Given
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Branch", DocNumber = "123", CompanyName = "Co" };
        var command = new EvaluateBranchCommand { BranchId = branch.Id, UserId = Guid.NewGuid(), Rate = 4, Comment = "Good" };

        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchEvaluationRepository.GetByBranchAndUserAsync(branch.Id, command.UserId, Arg.Any<CancellationToken>())
            .Returns((BranchEvaluation?)null);
        _branchEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BranchEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BranchEvaluation> { new() { BranchId = branch.Id, UserId = command.UserId, Rate = 4 } });
        _branchRateRepository.GetByBranchIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns((BranchRate?)null);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.AverageRate.Should().Be(4);
        result.ReviewCount.Should().Be(1);
        await _branchEvaluationRepository.Received(1).AddAsync(Arg.Any<BranchEvaluation>(), Arg.Any<CancellationToken>());
        await _branchRateRepository.Received(1).AddAsync(Arg.Any<BranchRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given branch already evaluated by user When evaluating again Then updates the existing evaluation")]
    public async Task Handle_ExistingEvaluation_UpdatesInsteadOfDuplicating()
    {
        // Given
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Branch", DocNumber = "123", CompanyName = "Co" };
        var userId = Guid.NewGuid();
        var existingEvaluation = new BranchEvaluation { Id = Guid.NewGuid(), BranchId = branch.Id, UserId = userId, Rate = 2, Comment = "Old" };
        var command = new EvaluateBranchCommand { BranchId = branch.Id, UserId = userId, Rate = 5, Comment = "Updated" };

        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _branchEvaluationRepository.GetByBranchAndUserAsync(branch.Id, userId, Arg.Any<CancellationToken>())
            .Returns(existingEvaluation);
        _branchEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<BranchEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BranchEvaluation> { existingEvaluation });
        _branchRateRepository.GetByBranchIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(new BranchRate { BranchId = branch.Id, AverageRate = 2, ReviewCount = 1 });

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        existingEvaluation.Rate.Should().Be(5);
        existingEvaluation.Comment.Should().Be("Updated");
        await _branchEvaluationRepository.DidNotReceive().AddAsync(Arg.Any<BranchEvaluation>(), Arg.Any<CancellationToken>());
        await _branchEvaluationRepository.Received(1).UpdateAsync(existingEvaluation, Arg.Any<CancellationToken>());
        await _branchRateRepository.Received(1).UpdateAsync(Arg.Any<BranchRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a branch that does not exist When evaluating Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new EvaluateBranchCommand { BranchId = Guid.NewGuid(), UserId = Guid.NewGuid(), Rate = 3 };
        _branchRepository.GetByIdAsync(command.BranchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
