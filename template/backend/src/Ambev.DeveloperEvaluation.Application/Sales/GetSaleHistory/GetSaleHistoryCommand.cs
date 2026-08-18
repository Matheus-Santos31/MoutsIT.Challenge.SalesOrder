using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleHistory;

public class GetSaleHistoryCommand : IRequest<IEnumerable<GetSaleHistoryResult>>
{
    public Guid UserId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
