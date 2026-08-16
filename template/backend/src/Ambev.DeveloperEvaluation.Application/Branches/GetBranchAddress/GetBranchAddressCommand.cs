using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;

public record GetBranchAddressCommand(Guid BranchId) : IRequest<GetBranchAddressResult>;
