using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;

public class CreateBranchAddressHandler : IRequestHandler<CreateBranchAddressCommand, BranchAddressResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IBranchAddressRepository _branchAddressRepository;

    public CreateBranchAddressHandler(
        IBranchRepository branchRepository,
        IAddressRepository addressRepository,
        IBranchAddressRepository branchAddressRepository)
    {
        _branchRepository = branchRepository;
        _addressRepository = addressRepository;
        _branchAddressRepository = branchAddressRepository;
    }

    public async Task<BranchAddressResult> Handle(CreateBranchAddressCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateBranchAddressValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(command.BranchId, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {command.BranchId} not found");

        var existing = await _branchAddressRepository.GetByBranchIdAsync(command.BranchId, cancellationToken);
        if (existing != null)
            throw new DomainException("This branch already has an address. Use the update endpoint instead.");

        var address = new Address
        {
            City = command.City,
            Street = command.Street,
            Number = command.Number,
            PostalCode = command.PostalCode,
            Latitude = command.Latitude,
            Longitude = command.Longitude
        };

        var branchAddress = new BranchAddress
        {
            BranchId = command.BranchId,
            Address = address
        };

        await _addressRepository.AddAsync(address, cancellationToken);
        await _branchAddressRepository.AddAsync(branchAddress, cancellationToken);
        await _branchAddressRepository.SaveChangesAsync(cancellationToken);

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
