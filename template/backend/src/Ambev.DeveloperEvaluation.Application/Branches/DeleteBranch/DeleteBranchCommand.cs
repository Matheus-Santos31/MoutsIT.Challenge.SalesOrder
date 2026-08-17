using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranch;

public class DeleteBranchCommand : IRequest<DeleteBranchResponse>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }

    public DeleteBranchCommand() { }

    public DeleteBranchCommand(Guid id) => Id = id;
}
