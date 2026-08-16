using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.ListProductEvaluations;

public class ListProductEvaluationsProfile : Profile
{
    public ListProductEvaluationsProfile()
    {
        CreateMap<ProductEvaluationItemResult, ProductEvaluationItemResponse>();
    }
}
