using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Branches.GetBranch;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.GetBranch;

public class GetBranchProfile : Profile
{
    public GetBranchProfile()
    {
        CreateMap<Guid, GetBranchCommand>()
            .ConstructUsing(id => new GetBranchCommand(id));

        CreateMap<GetBranchResult, GetBranchResponse>();
    }
}
