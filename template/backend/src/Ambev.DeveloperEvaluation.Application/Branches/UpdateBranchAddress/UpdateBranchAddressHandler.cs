using Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.UpdateBranchAddress;

public class UpdateBranchAddressHandler : IRequestHandler<UpdateBranchAddressCommand, BranchAddressResult>
{
    private readonly IBranchAddressRepository _branchAddressRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IBranchManagerRepository _branchManagerRepository;

    public UpdateBranchAddressHandler(
        IBranchAddressRepository branchAddressRepository,
        IAddressRepository addressRepository,
        IBranchManagerRepository branchManagerRepository)
    {
        _branchAddressRepository = branchAddressRepository;
        _addressRepository = addressRepository;
        _branchManagerRepository = branchManagerRepository;
    }

    public async Task<BranchAddressResult> Handle(UpdateBranchAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateBranchAddressValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!command.IsRequestingUserAdmin
            && !await _branchManagerRepository.IsManagerOfBranchAsync(command.RequestingUserId, command.BranchId, cancellationToken))
            throw new UnauthorizedAccessException("You can only manage branches you're assigned to.");

        var branchAddress = await _branchAddressRepository.GetByBranchIdAsync(command.BranchId, cancellationToken);
        if (branchAddress is null)
            throw new KeyNotFoundException($"No address found for branch {command.BranchId}");

        var address = await _addressRepository.GetByIdAsync(branchAddress.AddressId, cancellationToken);
        if (address is null)
            throw new KeyNotFoundException($"Address with ID {branchAddress.AddressId} not found");

        address.City = command.City;
        address.Street = command.Street;
        address.Number = command.Number;
        address.PostalCode = command.PostalCode;
        address.Latitude = command.Latitude;
        address.Longitude = command.Longitude;

        await _addressRepository.UpdateAsync(address, cancellationToken);
        await _addressRepository.SaveChangesAsync(cancellationToken);

        return new BranchAddressResult
        {
            Id = branchAddress.Id,
            BranchId = branchAddress.BranchId,
            AddressId = address.Id,
            City = address.City,
            Street = address.Street,
            Number = address.Number,
            PostalCode = address.PostalCode,
            Latitude = address.Latitude,
            Longitude = address.Longitude
        };
    }
}
