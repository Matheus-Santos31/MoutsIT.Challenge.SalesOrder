using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.EvaluateBranch;

public class EvaluateBranchRequestValidator : AbstractValidator<EvaluateBranchRequest>
{
    public EvaluateBranchRequestValidator()
    {
        RuleFor(x => x.Rate).InclusiveBetween(0, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
