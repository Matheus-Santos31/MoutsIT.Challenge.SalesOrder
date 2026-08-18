using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ListProductsHandler"/> class, covering pagination
/// and the merge of each product with its denormalized rate.
/// </summary>
public class ListProductsHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IProductRateRepository _productRateRepository;
    private readonly ListProductsHandler _handler;

    public ListProductsHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _productRateRepository = Substitute.For<IProductRateRepository>();
        _handler = new ListProductsHandler(_productRepository, _productRateRepository);
    }

    [Fact(DisplayName = "Given products with rates When listing Then merges each rate by product id")]
    public async Task Handle_ProductsWithRates_MergesRatesByProductId()
    {
        var productA = new Product { Id = Guid.NewGuid(), Title = "A" };
        var productB = new Product { Id = Guid.NewGuid(), Title = "B" };

        _productRepository.GetPagedAsync(1, 10, Arg.Any<IEnumerable<System.Linq.Expressions.Expression<Func<Product, bool>>>>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((new List<Product> { productA, productB }, 2));

        _productRateRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductRate, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductRate> { new() { ProductId = productA.Id, AverageRate = 3m, ReviewCount = 2 } });

        var result = await _handler.Handle(new ListProductsCommand { Page = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(x => x.Id == productA.Id && x.AverageRate == 3m);
        result.Items.Should().Contain(x => x.Id == productB.Id && x.AverageRate == null);
    }

    [Fact(DisplayName = "Given an invalid page size When listing Then throws ValidationException")]
    public async Task Handle_InvalidPageSize_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new ListProductsCommand { Page = 1, PageSize = 0 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
