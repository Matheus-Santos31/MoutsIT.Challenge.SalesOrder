using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Branches.EvaluateBranch;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.EvaluateBranch;

public class EvaluateBranchProfile : Profile
{
    public EvaluateBranchProfile()
    {
        CreateMap<EvaluateBranchRequest, EvaluateBranchCommand>();
        CreateMap<EvaluateBranchResult, EvaluateBranchResponse>();
    }
}
