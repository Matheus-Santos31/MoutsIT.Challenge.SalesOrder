namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.EvaluateBranch;

public class EvaluateBranchResponse
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal AverageRate { get; set; }
    public int ReviewCount { get; set; }
}
