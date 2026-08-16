using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.CreateUserAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUserAddress;

public class CreateUserAddressProfile : Profile
{
    public CreateUserAddressProfile()
    {
        CreateMap<CreateUserAddressRequest, CreateUserAddressCommand>();
        CreateMap<CreateUserAddressResult, CreateUserAddressResponse>();
    }
}
