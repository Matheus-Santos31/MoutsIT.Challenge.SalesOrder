using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateCartHandler"/> class.
/// </summary>
public class CreateCartHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly CreateCartHandler _handler;

    public CreateCartHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateCartHandler(_branchRepository, _cartRepository, _mapper);
    }

    [Fact(DisplayName = "Given a valid branch When creating a cart Then opens it as Active and empty")]
    public async Task Handle_ValidBranch_CreatesActiveEmptyCart()
    {
        var branch = new Branch { Id = Guid.NewGuid() };
        var command = new CreateCartCommand { BranchId = branch.Id, UserId = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        _mapper.Map<CreateCartResult>(Arg.Any<Cart>()).Returns(new CreateCartResult { Status = CartStatus.Active });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(CartStatus.Active);
        await _cartRepository.Received(1).AddAsync(
            Arg.Is<Cart>(c => c.BranchId == branch.Id && c.UserId == command.UserId && c.Status == CartStatus.Active && c.TotalItems == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a branch that does not exist When creating a cart Then throws KeyNotFoundException")]
    public async Task Handle_BranchNotFound_ThrowsKeyNotFoundException()
    {
        var command = new CreateCartCommand { BranchId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _branchRepository.GetByIdAsync(command.BranchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an empty branch id When creating a cart Then throws ValidationException")]
    public async Task Handle_EmptyBranchId_ThrowsValidationException()
    {
        var command = new CreateCartCommand { BranchId = Guid.Empty, UserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
