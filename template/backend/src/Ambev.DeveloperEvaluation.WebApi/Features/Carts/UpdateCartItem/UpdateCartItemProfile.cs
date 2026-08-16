using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCartItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCartItem;

public class UpdateCartItemProfile : Profile
{
    public UpdateCartItemProfile()
    {
        CreateMap<UpdateCartItemRequest, UpdateCartItemCommand>();
    }
}
