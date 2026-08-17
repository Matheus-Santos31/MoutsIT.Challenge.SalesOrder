using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Branches.AssignBranchManager;

public class AssignBranchManagerValidator : AbstractValidator<AssignBranchManagerCommand>
{
    public AssignBranchManagerValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
