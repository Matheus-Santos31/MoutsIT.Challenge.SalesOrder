using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Shared item/address mappings reused by every Sales feature's Response.
/// </summary>
public class SharedSalesProfile : Profile
{
    public SharedSalesProfile()
    {
        CreateMap<SaleItemResult, SaleItemResponse>();
        CreateMap<SaleAddressResult, SaleAddressResponse>();
    }
}
