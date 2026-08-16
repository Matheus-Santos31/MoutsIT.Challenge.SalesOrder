using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Branches.EvaluateBranch;

public class EvaluateBranchValidator : AbstractValidator<EvaluateBranchCommand>
{
    public EvaluateBranchValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rate).InclusiveBetween(0, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
