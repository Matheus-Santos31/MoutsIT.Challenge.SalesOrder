using Ambev.DeveloperEvaluation.Application.Products.DeleteProductEvaluation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteProductEvaluationHandler"/> class, covering
/// the rate recalculation and the delete-the-aggregate-when-empty rule.
/// </summary>
public class DeleteProductEvaluationHandlerTests
{
    private readonly IProductEvaluationRepository _productEvaluationRepository;
    private readonly IProductRateRepository _productRateRepository;
    private readonly DeleteProductEvaluationHandler _handler;

    public DeleteProductEvaluationHandlerTests()
    {
        _productEvaluationRepository = Substitute.For<IProductEvaluationRepository>();
        _productRateRepository = Substitute.For<IProductRateRepository>();
        _handler = new DeleteProductEvaluationHandler(_productEvaluationRepository, _productRateRepository);
    }

    [Fact(DisplayName = "Given other evaluations remain When deleting one Then recalculates the rate instead of deleting it")]
    public async Task Handle_OtherEvaluationsRemain_RecalculatesRate()
    {
        var productId = Guid.NewGuid();
        var evaluation = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = productId, Rate = 2 };
        var remaining = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = productId, Rate = 4 };
        var rate = new ProductRate { ProductId = productId, AverageRate = 3, ReviewCount = 2 };

        _productEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);
        _productEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductEvaluation> { remaining });
        _productRateRepository.GetByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(rate);

        var result = await _handler.Handle(new DeleteProductEvaluationCommand(productId, evaluation.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        rate.AverageRate.Should().Be(4);
        rate.ReviewCount.Should().Be(1);
        await _productRateRepository.Received(1).UpdateAsync(rate, Arg.Any<CancellationToken>());
        await _productRateRepository.DidNotReceive().DeleteAsync(Arg.Any<ProductRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given no evaluations remain When deleting the last one Then deletes the rate aggregate too")]
    public async Task Handle_LastEvaluation_DeletesRateAggregate()
    {
        var productId = Guid.NewGuid();
        var evaluation = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = productId, Rate = 2 };
        var rate = new ProductRate { ProductId = productId, AverageRate = 2, ReviewCount = 1 };

        _productEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);
        _productEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductEvaluation>());
        _productRateRepository.GetByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(rate);

        var result = await _handler.Handle(new DeleteProductEvaluationCommand(productId, evaluation.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        await _productRateRepository.Received(1).DeleteAsync(rate, Arg.Any<CancellationToken>());
        await _productRateRepository.DidNotReceive().UpdateAsync(Arg.Any<ProductRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an evaluation that does not belong to the product When deleting Then throws KeyNotFoundException")]
    public async Task Handle_EvaluationBelongsToDifferentProduct_ThrowsKeyNotFoundException()
    {
        var evaluation = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = Guid.NewGuid() };
        _productEvaluationRepository.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>()).Returns(evaluation);

        var act = () => _handler.Handle(new DeleteProductEvaluationCommand(Guid.NewGuid(), evaluation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an evaluation that does not exist When deleting Then throws KeyNotFoundException")]
    public async Task Handle_EvaluationNotFound_ThrowsKeyNotFoundException()
    {
        var evaluationId = Guid.NewGuid();
        _productEvaluationRepository.GetByIdAsync(evaluationId, Arg.Any<CancellationToken>()).Returns((ProductEvaluation?)null);

        var act = () => _handler.Handle(new DeleteProductEvaluationCommand(Guid.NewGuid(), evaluationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
