using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProductEvaluation;

public class DeleteProductEvaluationHandler : IRequestHandler<DeleteProductEvaluationCommand, DeleteProductEvaluationResponse>
{
    private readonly IProductEvaluationRepository _productEvaluationRepository;
    private readonly IProductRateRepository _productRateRepository;

    public DeleteProductEvaluationHandler(IProductEvaluationRepository productEvaluationRepository, IProductRateRepository productRateRepository)
    {
        _productEvaluationRepository = productEvaluationRepository;
        _productRateRepository = productRateRepository;
    }

    public async Task<DeleteProductEvaluationResponse> Handle(DeleteProductEvaluationCommand request, CancellationToken cancellationToken)
    {
        var evaluation = await _productEvaluationRepository.GetByIdAsync(request.EvaluationId, cancellationToken);
        if (evaluation is null || evaluation.ProductId != request.ProductId)
            throw new KeyNotFoundException($"Evaluation with ID {request.EvaluationId} not found for this product");

        await _productEvaluationRepository.DeleteAsync(evaluation, cancellationToken);
        await _productEvaluationRepository.SaveChangesAsync(cancellationToken);

        var remaining = (await _productEvaluationRepository.GetAsync(x => x.ProductId == request.ProductId, cancellationToken)).ToList();
        var rate = await _productRateRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (rate is not null)
        {
            if (remaining.Count == 0)
            {
                await _productRateRepository.DeleteAsync(rate, cancellationToken);
            }
            else
            {
                rate.AverageRate = remaining.Average(x => x.Rate);
                rate.ReviewCount = remaining.Count;
                await _productRateRepository.UpdateAsync(rate, cancellationToken);
            }

            await _productRateRepository.SaveChangesAsync(cancellationToken);
        }

        return new DeleteProductEvaluationResponse { Success = true };
    }
}
