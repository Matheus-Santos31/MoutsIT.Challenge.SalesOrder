using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public class GetProductHandler : IRequestHandler<GetProductCommand, GetProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductRateRepository _productRateRepository;
    private readonly IMapper _mapper;

    public GetProductHandler(IProductRepository productRepository, IProductRateRepository productRateRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _productRateRepository = productRateRepository;
        _mapper = mapper;
    }

    public async Task<GetProductResult> Handle(GetProductCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetProductValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID {request.Id} not found");

        var result = _mapper.Map<GetProductResult>(product);

        var rate = await _productRateRepository.GetByProductIdAsync(request.Id, cancellationToken);
        if (rate is not null)
        {
            result.AverageRate = rate.AverageRate;
            result.ReviewCount = rate.ReviewCount;
        }

        return result;
    }
}
