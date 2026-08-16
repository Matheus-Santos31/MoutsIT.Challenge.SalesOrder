using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;

public record DeleteBranchAddressCommand(Guid BranchId) : IRequest<DeleteBranchAddressResponse>;
