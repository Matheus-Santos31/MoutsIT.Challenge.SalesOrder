using Ambev.DeveloperEvaluation.Application.Branches.CreateBranch;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateBranchHandler"/> class.
/// </summary>
public class CreateBranchHandlerTests
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly CreateBranchHandler _handler;

    public CreateBranchHandlerTests()
    {
        _branchRepository = Substitute.For<IBranchRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateBranchHandler(_branchRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid branch data When creating Then persists and returns the mapped result")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = BranchHandlerTestData.GenerateValidCreateCommand();
        var branch = new Branch { Id = Guid.NewGuid(), Name = command.Name, DocNumber = command.DocNumber, CompanyName = command.CompanyName };

        _branchRepository.GetByDocNumberAsync(command.DocNumber, Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns((Branch?)null);
        _mapper.Map<Branch>(command).Returns(branch);
        _mapper.Map<CreateBranchResult>(branch).Returns(new CreateBranchResult { Id = branch.Id, Name = branch.Name });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(branch.Id);
        await _branchRepository.Received(1).AddAsync(branch, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a document number already in use When creating Then throws DomainException")]
    public async Task Handle_DuplicateDocNumber_ThrowsDomainException()
    {
        var command = BranchHandlerTestData.GenerateValidCreateCommand();
        _branchRepository.GetByDocNumberAsync(command.DocNumber, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new Branch { Id = Guid.NewGuid(), DocNumber = command.DocNumber });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact(DisplayName = "Given an empty name When creating Then throws ValidationException")]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        var command = BranchHandlerTestData.GenerateValidCreateCommand();
        command.Name = string.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
