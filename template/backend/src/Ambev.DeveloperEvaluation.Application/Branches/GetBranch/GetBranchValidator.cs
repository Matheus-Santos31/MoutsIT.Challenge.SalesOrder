using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranch;

public class GetBranchValidator : AbstractValidator<GetBranchCommand>
{
    public GetBranchValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
