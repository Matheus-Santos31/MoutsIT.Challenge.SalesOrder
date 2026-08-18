using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleHistory;

public class GetSaleHistoryValidator : AbstractValidator<GetSaleHistoryCommand>
{
    public GetSaleHistoryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
