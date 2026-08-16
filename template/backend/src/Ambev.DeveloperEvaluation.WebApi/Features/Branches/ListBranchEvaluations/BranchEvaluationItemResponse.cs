namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.ListBranchEvaluations;

public class BranchEvaluationItemResponse
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
