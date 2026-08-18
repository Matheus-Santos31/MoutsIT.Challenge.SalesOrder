using Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListProductEvaluationsHandler"/> class.
/// </summary>
public class ListProductEvaluationsHandlerTests
{
    private readonly IProductEvaluationRepository _productEvaluationRepository;
    private readonly ListProductEvaluationsHandler _handler;

    public ListProductEvaluationsHandlerTests()
    {
        _productEvaluationRepository = Substitute.For<IProductEvaluationRepository>();
        _handler = new ListProductEvaluationsHandler(_productEvaluationRepository);
    }

    [Fact(DisplayName = "Given a product with evaluations When listing Then returns the paged items")]
    public async Task Handle_ProductWithEvaluations_ReturnsPagedItems()
    {
        var productId = Guid.NewGuid();
        var evaluation = new ProductEvaluation { Id = Guid.NewGuid(), ProductId = productId, UserId = Guid.NewGuid(), Rate = 4, Comment = "Nice" };

        _productEvaluationRepository.GetPagedAsync(1, 10, Arg.Any<IEnumerable<System.Linq.Expressions.Expression<Func<ProductEvaluation, bool>>>>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((new List<ProductEvaluation> { evaluation }, 1));

        var command = new ListProductEvaluationsCommand { ProductId = productId, Page = 1, PageSize = 10 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(x => x.Id == evaluation.Id && x.Rate == 4);
    }

    [Fact(DisplayName = "Given an empty product id When listing Then throws ValidationException")]
    public async Task Handle_EmptyProductId_ThrowsValidationException()
    {
        var command = new ListProductEvaluationsCommand { ProductId = Guid.Empty, Page = 1, PageSize = 10 };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
