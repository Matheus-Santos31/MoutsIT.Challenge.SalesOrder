using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartHandler : IRequestHandler<CreateCartCommand, CreateCartResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public CreateCartHandler(IBranchRepository branchRepository, ICartRepository cartRepository, IMapper mapper)
    {
        _branchRepository = branchRepository;
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CreateCartResult> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateCartValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var branch = await _branchRepository.GetByIdAsync(command.BranchId, cancellationToken);
        if (branch is null)
            throw new KeyNotFoundException($"Branch with ID {command.BranchId} not found");

        var cart = new Cart
        {
            BranchId = command.BranchId,
            UserId = command.UserId,
            Status = CartStatus.Active,
            TotalItems = 0,
            TotalAmount = 0
        };

        await _cartRepository.AddAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CreateCartResult>(cart);
    }
}
