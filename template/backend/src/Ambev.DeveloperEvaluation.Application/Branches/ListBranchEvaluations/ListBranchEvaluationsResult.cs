namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranchEvaluations;

public class ListBranchEvaluationsResult
{
    public IEnumerable<BranchEvaluationItemResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class BranchEvaluationItemResult
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
}
