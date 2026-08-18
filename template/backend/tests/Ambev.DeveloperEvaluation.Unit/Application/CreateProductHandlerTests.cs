using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
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
/// Contains unit tests for the <see cref="CreateProductHandler"/> class.
/// </summary>
public class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid product data When creating Then persists and returns the mapped result")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = ProductHandlerTestData.GenerateValidCreateCommand();
        var product = new Product { Id = Guid.NewGuid(), Title = command.Title, Price = command.Price, Description = command.Description, Category = command.Category, Image = command.Image };
        var result = new CreateProductResult { Id = product.Id, Title = product.Title, Price = product.Price };

        _mapper.Map<Product>(command).Returns(product);
        _mapper.Map<CreateProductResult>(product).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Id.Should().Be(product.Id);
        await _productRepository.Received(1).AddAsync(product, Arg.Any<CancellationToken>());
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a product with Unspecified category When creating Then throws ValidationException")]
    public async Task Handle_UnspecifiedCategory_ThrowsValidationException()
    {
        var command = ProductHandlerTestData.GenerateValidCreateCommand();
        command.Category = Ambev.DeveloperEvaluation.Domain.Enums.ProductCategory.Unspecified;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given a non-positive price When creating Then throws ValidationException")]
    public async Task Handle_NonPositivePrice_ThrowsValidationException()
    {
        var command = ProductHandlerTestData.GenerateValidCreateCommand();
        command.Price = 0;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
