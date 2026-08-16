namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.GetBranch;

public class GetBranchResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal? AverageRate { get; set; }
    public int? ReviewCount { get; set; }
}
