using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.CartItem;

public class CartItemProfile : Profile
{
    public CartItemProfile()
    {
        CreateMap<CartItemResult, CartItemResponse>();
    }
}
