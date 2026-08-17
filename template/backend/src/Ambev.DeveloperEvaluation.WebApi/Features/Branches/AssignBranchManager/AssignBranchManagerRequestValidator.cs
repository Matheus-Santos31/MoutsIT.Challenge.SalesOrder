using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.AssignBranchManager;

public class AssignBranchManagerRequestValidator : AbstractValidator<AssignBranchManagerRequest>
{
    public AssignBranchManagerRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
