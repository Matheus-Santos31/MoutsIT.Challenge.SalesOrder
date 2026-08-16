using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.UpdateBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.BranchAddress;

public class BranchAddressProfile : Profile
{
    public BranchAddressProfile()
    {
        CreateMap<BranchAddressRequest, CreateBranchAddressCommand>();
        CreateMap<BranchAddressRequest, UpdateBranchAddressCommand>();
        CreateMap<BranchAddressResult, BranchAddressResponse>();
        CreateMap<GetBranchAddressResult, BranchAddressResponse>();
    }
}
