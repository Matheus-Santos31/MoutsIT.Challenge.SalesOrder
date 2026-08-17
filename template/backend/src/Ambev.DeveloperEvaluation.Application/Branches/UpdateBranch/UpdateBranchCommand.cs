using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.UpdateBranch;

public class UpdateBranchCommand : IRequest<UpdateBranchResult>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}
