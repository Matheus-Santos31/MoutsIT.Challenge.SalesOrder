using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.GetCart;

public class GetCartProfile : Profile
{
    public GetCartProfile()
    {
        CreateMap<CartItemDetail, CartItemDetailResponse>();
        CreateMap<GetCartResult, GetCartResponse>();
    }
}
