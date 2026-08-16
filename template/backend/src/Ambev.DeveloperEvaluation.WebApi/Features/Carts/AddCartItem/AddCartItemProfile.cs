using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.AddCartItem;

public class AddCartItemProfile : Profile
{
    public AddCartItemProfile()
    {
        CreateMap<AddCartItemRequest, AddCartItemCommand>();
    }
}
