using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.SetDefaultUserAddress;

public class SetDefaultUserAddressProfile : Profile
{
    public SetDefaultUserAddressProfile()
    {
        CreateMap<SetDefaultUserAddressResult, SetDefaultUserAddressResponse>();
    }
}
