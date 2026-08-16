using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;

public class ListBranchEvaluationsValidator : AbstractValidator<ListBranchEvaluationsCommand>
{
    public ListBranchEvaluationsValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
