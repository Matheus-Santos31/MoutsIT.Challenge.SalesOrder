using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteProductHandler"/> class.
/// </summary>
public class DeleteProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new DeleteProductHandler(_productRepository);
    }

    [Fact(DisplayName = "Given an existing product When deleting Then soft-deletes and returns success")]
    public async Task Handle_ExistingProduct_DeletesAndReturnsSuccess()
    {
        var product = new Product { Id = Guid.NewGuid() };
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        await _productRepository.Received(1).DeleteAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a product that does not exist When deleting Then throws KeyNotFoundException")]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty id When deleting Then throws ValidationException")]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new DeleteProductCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
