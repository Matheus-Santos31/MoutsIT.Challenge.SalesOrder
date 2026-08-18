using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateProductHandler"/> class.
/// </summary>
public class UpdateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid data for an existing product When updating Then applies changes and saves")]
    public async Task Handle_ValidRequest_UpdatesProduct()
    {
        var product = new Product { Id = Guid.NewGuid(), Title = "Old", Price = 1m, Category = Ambev.DeveloperEvaluation.Domain.Enums.ProductCategory.Food };
        var command = ProductHandlerTestData.GenerateValidUpdateCommand(product.Id);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _mapper.Map<UpdateProductResult>(product).Returns(new UpdateProductResult { Id = product.Id, Title = command.Title });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be(command.Title);
        product.Title.Should().Be(command.Title);
        product.Price.Should().Be(command.Price);
        product.Category.Should().Be(command.Category);
        await _productRepository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a product that does not exist When updating Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var command = ProductHandlerTestData.GenerateValidUpdateCommand();
        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty title When updating Then throws ValidationException")]
    public async Task Handle_EmptyTitle_ThrowsValidationException()
    {
        var command = ProductHandlerTestData.GenerateValidUpdateCommand();
        command.Title = string.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
