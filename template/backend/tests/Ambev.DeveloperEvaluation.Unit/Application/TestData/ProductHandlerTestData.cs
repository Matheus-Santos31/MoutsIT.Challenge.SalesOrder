using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides Bogus-based generators for Product-related handler commands, centralizing test
/// data generation so handler tests stay focused on behavior instead of fixture setup.
/// </summary>
public static class ProductHandlerTestData
{
    private static readonly ProductCategory[] realCategories =
    [
        ProductCategory.Electronics, ProductCategory.Home, ProductCategory.Fashion,
        ProductCategory.Food, ProductCategory.Sports, ProductCategory.Office
    ];

    private static readonly Faker<CreateProductCommand> createProductFaker = new Faker<CreateProductCommand>()
        .RuleFor(x => x.Title, f => f.Commerce.ProductName())
        .RuleFor(x => x.Price, f => f.Random.Decimal(1, 999))
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.Category, f => f.PickRandom(realCategories))
        .RuleFor(x => x.Image, f => f.Image.PicsumUrl());

    private static readonly Faker<UpdateProductCommand> updateProductFaker = new Faker<UpdateProductCommand>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Title, f => f.Commerce.ProductName())
        .RuleFor(x => x.Price, f => f.Random.Decimal(1, 999))
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.Category, f => f.PickRandom(realCategories))
        .RuleFor(x => x.Image, f => f.Image.PicsumUrl());

    private static readonly Faker<EvaluateProductCommand> evaluateProductFaker = new Faker<EvaluateProductCommand>()
        .RuleFor(x => x.ProductId, f => f.Random.Guid())
        .RuleFor(x => x.UserId, f => f.Random.Guid())
        .RuleFor(x => x.Rate, f => f.Random.Decimal(0, 5))
        .RuleFor(x => x.Comment, f => f.Rant.Review());

    public static CreateProductCommand GenerateValidCreateCommand() => createProductFaker.Generate();

    public static UpdateProductCommand GenerateValidUpdateCommand(Guid? id = null)
    {
        var command = updateProductFaker.Generate();
        if (id.HasValue)
            command.Id = id.Value;

        return command;
    }

    public static EvaluateProductCommand GenerateValidEvaluateCommand(Guid? productId = null, Guid? userId = null)
    {
        var command = evaluateProductFaker.Generate();
        if (productId.HasValue)
            command.ProductId = productId.Value;
        if (userId.HasValue)
            command.UserId = userId.Value;

        return command;
    }
}
