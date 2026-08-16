namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.EvaluateBranch;

public class EvaluateBranchRequest
{
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
