using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetProductHandler"/> class, covering the merge of
/// the product record with its denormalized rate aggregate.
/// </summary>
public class GetProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IProductRateRepository _productRateRepository;
    private readonly IMapper _mapper;
    private readonly GetProductHandler _handler;

    public GetProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _productRateRepository = Substitute.For<IProductRateRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetProductHandler(_productRepository, _productRateRepository, _mapper);
    }

    [Fact(DisplayName = "Given a product with an existing rate When getting Then merges rate into the result")]
    public async Task Handle_ProductWithRate_MergesRateIntoResult()
    {
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer" };
        var rate = new ProductRate { ProductId = product.Id, AverageRate = 4.5m, ReviewCount = 10 };
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRateRepository.GetByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(rate);
        _mapper.Map<GetProductResult>(product).Returns(new GetProductResult { Id = product.Id, Title = product.Title });

        var result = await _handler.Handle(new GetProductCommand(product.Id), CancellationToken.None);

        result.AverageRate.Should().Be(4.5m);
        result.ReviewCount.Should().Be(10);
    }

    [Fact(DisplayName = "Given a product with no rate yet When getting Then leaves rate fields null")]
    public async Task Handle_ProductWithoutRate_LeavesRateFieldsNull()
    {
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer" };
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRateRepository.GetByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns((ProductRate?)null);
        _mapper.Map<GetProductResult>(product).Returns(new GetProductResult { Id = product.Id, Title = product.Title });

        var result = await _handler.Handle(new GetProductCommand(product.Id), CancellationToken.None);

        result.AverageRate.Should().BeNull();
        result.ReviewCount.Should().BeNull();
    }

    [Fact(DisplayName = "Given a product that does not exist When getting Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _handler.Handle(new GetProductCommand(productId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty id When getting Then throws ValidationException")]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetProductCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
