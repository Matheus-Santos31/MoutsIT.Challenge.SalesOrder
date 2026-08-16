namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.UpdateBranch;

public class UpdateBranchResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}
