namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranches;

public class ListBranchesResult
{
    public IEnumerable<BranchListItemResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class BranchListItemResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal? AverageRate { get; set; }
    public int? ReviewCount { get; set; }
}
