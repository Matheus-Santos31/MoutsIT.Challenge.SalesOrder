using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUserAddress;

public class UpdateUserAddressProfile : Profile
{
    public UpdateUserAddressProfile()
    {
        CreateMap<UpdateUserAddressRequest, UpdateUserAddressCommand>();
        CreateMap<UpdateUserAddressResult, UpdateUserAddressResponse>();
    }
}
