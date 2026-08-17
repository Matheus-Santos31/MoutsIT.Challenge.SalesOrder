using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;

public class DeleteBranchAddressCommand : IRequest<DeleteBranchAddressResponse>
{
    public Guid BranchId { get; set; }
    public Guid RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }

    public DeleteBranchAddressCommand() { }

    public DeleteBranchAddressCommand(Guid branchId) => BranchId = branchId;
}
