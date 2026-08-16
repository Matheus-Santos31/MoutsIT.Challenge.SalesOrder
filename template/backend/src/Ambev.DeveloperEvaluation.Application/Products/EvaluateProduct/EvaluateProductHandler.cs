using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;

public class EvaluateProductHandler : IRequestHandler<EvaluateProductCommand, EvaluateProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductEvaluationRepository _productEvaluationRepository;
    private readonly IProductRateRepository _productRateRepository;

    public EvaluateProductHandler(
        IProductRepository productRepository,
        IProductEvaluationRepository productEvaluationRepository,
        IProductRateRepository productRateRepository)
    {
        _productRepository = productRepository;
        _productEvaluationRepository = productEvaluationRepository;
        _productRateRepository = productRateRepository;
    }

    public async Task<EvaluateProductResult> Handle(EvaluateProductCommand command, CancellationToken cancellationToken)
    {
        var validator = new EvaluateProductValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID {command.ProductId} not found");

        var evaluation = await _productEvaluationRepository.GetByProductAndUserAsync(command.ProductId, command.UserId, cancellationToken);
        if (evaluation is null)
        {
            evaluation = new ProductEvaluation
            {
                ProductId = command.ProductId,
                UserId = command.UserId,
                Rate = command.Rate,
                Comment = command.Comment
            };
            await _productEvaluationRepository.AddAsync(evaluation, cancellationToken);
        }
        else
        {
            evaluation.Rate = command.Rate;
            evaluation.Comment = command.Comment;
            await _productEvaluationRepository.UpdateAsync(evaluation, cancellationToken);
        }

        await _productEvaluationRepository.SaveChangesAsync(cancellationToken);

        var allEvaluations = await _productEvaluationRepository.GetAsync(x => x.ProductId == command.ProductId, cancellationToken);
        var evaluationList = allEvaluations.ToList();
        var averageRate = evaluationList.Average(x => x.Rate);
        var reviewCount = evaluationList.Count;

        var rate = await _productRateRepository.GetByProductIdAsync(command.ProductId, cancellationToken);
        if (rate is null)
        {
            rate = new ProductRate { ProductId = command.ProductId, AverageRate = averageRate, ReviewCount = reviewCount };
            await _productRateRepository.AddAsync(rate, cancellationToken);
        }
        else
        {
            rate.AverageRate = averageRate;
            rate.ReviewCount = reviewCount;
            await _productRateRepository.UpdateAsync(rate, cancellationToken);
        }

        await _productRateRepository.SaveChangesAsync(cancellationToken);

        return new EvaluateProductResult
        {
            Id = evaluation.Id,
            ProductId = evaluation.ProductId,
            UserId = evaluation.UserId,
            Rate = evaluation.Rate,
            Comment = evaluation.Comment,
            AverageRate = averageRate,
            ReviewCount = reviewCount
        };
    }
}
