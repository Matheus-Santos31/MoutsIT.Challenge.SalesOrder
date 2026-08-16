using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUserAddresses;

public class ListUserAddressesProfile : Profile
{
    public ListUserAddressesProfile()
    {
        CreateMap<UserAddressResult, ListUserAddressesResponse>();
    }
}
