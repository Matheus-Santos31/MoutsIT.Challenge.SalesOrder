using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.EvaluateProduct;

public class EvaluateProductProfile : Profile
{
    public EvaluateProductProfile()
    {
        CreateMap<EvaluateProductRequest, EvaluateProductCommand>();
        CreateMap<EvaluateProductResult, EvaluateProductResponse>();
    }
}
