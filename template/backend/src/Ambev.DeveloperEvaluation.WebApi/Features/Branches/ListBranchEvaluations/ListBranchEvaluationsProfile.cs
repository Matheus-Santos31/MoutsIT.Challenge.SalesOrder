using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.ListBranchEvaluations;

public class ListBranchEvaluationsProfile : Profile
{
    public ListBranchEvaluationsProfile()
    {
        CreateMap<BranchEvaluationItemResult, BranchEvaluationItemResponse>();
    }
}
