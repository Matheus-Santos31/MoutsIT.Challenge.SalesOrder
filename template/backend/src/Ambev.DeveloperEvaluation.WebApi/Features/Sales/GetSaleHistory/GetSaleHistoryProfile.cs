using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.GetSaleHistory;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleHistory;

public class GetSaleHistoryProfile : Profile
{
    public GetSaleHistoryProfile()
    {
        CreateMap<GetSaleHistoryResult, GetSaleHistoryResponse>();
        CreateMap<GetSaleHistoryItemResult, GetSaleHistoryItemResponse>();
    }
}
