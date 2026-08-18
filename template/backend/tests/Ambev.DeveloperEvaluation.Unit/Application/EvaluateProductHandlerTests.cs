using Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="EvaluateProductHandler"/> class, covering the
/// upsert-into-evaluation and rating recalculation rules.
/// </summary>
public class EvaluateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IProductEvaluationRepository _productEvaluationRepository;
    private readonly IProductRateRepository _productRateRepository;
    private readonly EvaluateProductHandler _handler;

    public EvaluateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _productEvaluationRepository = Substitute.For<IProductEvaluationRepository>();
        _productRateRepository = Substitute.For<IProductRateRepository>();
        _handler = new EvaluateProductHandler(_productRepository, _productEvaluationRepository, _productRateRepository);
    }

    [Fact(DisplayName = "Given product never evaluated by user When evaluating Then creates evaluation and rate")]
    public async Task Handle_FirstEvaluation_CreatesEvaluationAndRate()
    {
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer" };
        var command = ProductHandlerTestData.GenerateValidEvaluateCommand(product.Id);
        command.Rate = 4;

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productEvaluationRepository.GetByProductAndUserAsync(product.Id, command.UserId, Arg.Any<CancellationToken>())
            .Returns((ProductEvaluation?)null);
        _productEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductEvaluation> { new() { ProductId = product.Id, UserId = command.UserId, Rate = 4 } });
        _productRateRepository.GetByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns((ProductRate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AverageRate.Should().Be(4);
        result.ReviewCount.Should().Be(1);
        await _productEvaluationRepository.Received(1).AddAsync(Arg.Any<ProductEvaluation>(), Arg.Any<CancellationToken>());
        await _productRateRepository.Received(1).AddAsync(Arg.Any<ProductRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given product already evaluated by user When evaluating again Then updates the existing evaluation")]
    public async Task Handle_ExistingEvaluation_UpdatesInsteadOfDuplicating()
    {
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer" };
        var userId = Guid.NewGuid();
        var existingEvaluation = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = product.Id, UserId = userId, Rate = 2, Comment = "Old" };
        var command = ProductHandlerTestData.GenerateValidEvaluateCommand(product.Id, userId);
        command.Rate = 5;

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productEvaluationRepository.GetByProductAndUserAsync(product.Id, userId, Arg.Any<CancellationToken>())
            .Returns(existingEvaluation);
        _productEvaluationRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductEvaluation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductEvaluation> { existingEvaluation });
        _productRateRepository.GetByProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(new ProductRate { ProductId = product.Id, AverageRate = 2, ReviewCount = 1 });

        var result = await _handler.Handle(command, CancellationToken.None);

        existingEvaluation.Rate.Should().Be(5);
        await _productEvaluationRepository.DidNotReceive().AddAsync(Arg.Any<ProductEvaluation>(), Arg.Any<CancellationToken>());
        await _productEvaluationRepository.Received(1).UpdateAsync(existingEvaluation, Arg.Any<CancellationToken>());
        await _productRateRepository.Received(1).UpdateAsync(Arg.Any<ProductRate>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a product that does not exist When evaluating Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var command = ProductHandlerTestData.GenerateValidEvaluateCommand();
        _productRepository.GetByIdAsync(command.ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a rate outside 0-5 When evaluating Then throws ValidationException")]
    public async Task Handle_RateOutOfRange_ThrowsValidationException()
    {
        var command = ProductHandlerTestData.GenerateValidEvaluateCommand();
        command.Rate = 6;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
