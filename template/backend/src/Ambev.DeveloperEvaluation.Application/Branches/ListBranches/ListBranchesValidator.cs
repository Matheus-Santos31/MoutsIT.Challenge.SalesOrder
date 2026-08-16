using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranches;

public class ListBranchesValidator : AbstractValidator<ListBranchesCommand>
{
    public ListBranchesValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
