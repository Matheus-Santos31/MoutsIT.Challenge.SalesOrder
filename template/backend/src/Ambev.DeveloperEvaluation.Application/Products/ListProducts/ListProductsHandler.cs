using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsHandler : IRequestHandler<ListProductsCommand, ListProductsResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductRateRepository _productRateRepository;

    public ListProductsHandler(IProductRepository productRepository, IProductRateRepository productRateRepository)
    {
        _productRepository = productRepository;
        _productRateRepository = productRateRepository;
    }

    public async Task<ListProductsResult> Handle(ListProductsCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListProductsValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var filters = new List<Expression<Func<Product, bool>>>();
        if (command.Category.HasValue)
            filters.Add(x => x.Category == command.Category.Value);

        var (products, totalCount) = await _productRepository.GetPagedAsync(
            command.Page, command.PageSize, filters, command.OrderBy, command.Ascending, cancellationToken);

        var productList = products.ToList();
        var productIds = productList.Select(x => x.Id).ToList();

        var rates = await _productRateRepository.GetAsync(x => productIds.Contains(x.ProductId), cancellationToken);
        var ratesByProduct = rates.ToDictionary(x => x.ProductId);

        var items = productList.Select(product =>
        {
            ratesByProduct.TryGetValue(product.Id, out var rate);
            return new ProductListItemResult
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                Image = product.Image,
                AverageRate = rate?.AverageRate,
                ReviewCount = rate?.ReviewCount
            };
        });

        return new ListProductsResult { Items = items, TotalCount = totalCount };
    }
}
