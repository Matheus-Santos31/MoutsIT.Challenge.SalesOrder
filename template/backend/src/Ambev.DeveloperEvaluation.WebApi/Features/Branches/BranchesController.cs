using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Branches.CreateBranch;
using Ambev.DeveloperEvaluation.WebApi.Features.Branches.GetBranch;
using Ambev.DeveloperEvaluation.Application.Branches.CreateBranch;
using Ambev.DeveloperEvaluation.Application.Branches.GetBranch;
using Ambev.DeveloperEvaluation.WebApi.Features.Branches.ListBranches;
using Ambev.DeveloperEvaluation.WebApi.Features.Branches.UpdateBranch;
using Ambev.DeveloperEvaluation.Application.Branches.ListBranches;
using Ambev.DeveloperEvaluation.Application.Branches.UpdateBranch;
using Ambev.DeveloperEvaluation.Application.Branches.DeleteBranch;
using Ambev.DeveloperEvaluation.Application.Branches.CreateBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.DeleteBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.GetBranchAddress;
using Ambev.DeveloperEvaluation.Application.Branches.UpdateBranchAddress;
using Ambev.DeveloperEvaluation.WebApi.Features.Branches.BranchAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public BranchesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateBranchResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateBranchRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateBranchCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateBranchResponse>
        {
            Success = true,
            Message = "Branch created successfully",
            Data = _mapper.Map<CreateBranchResponse>(response)
        });
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetBranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranch([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetBranchCommand(id), cancellationToken);

        return Ok(new ApiResponseWithData<GetBranchResponse>
        {
            Success = true,
            Message = "Branch retrieved successfully",
            Data = _mapper.Map<GetBranchResponse>(response)
        });
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BranchListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBranches([FromQuery] ListBranchesRequest request, CancellationToken cancellationToken)
    {
        var (orderField, ascending) = OrderByParser.Parse(request.Order);

        var command = new ListBranchesCommand
        {
            Page = request.Page,
            PageSize = request.Size,
            OrderBy = orderField,
            Ascending = ascending
        };

        var result = await _mediator.Send(command, cancellationToken);
        var items = _mapper.Map<List<BranchListItemResponse>>(result.Items);

        return OkPaginated(new PaginatedList<BranchListItemResponse>(items, result.TotalCount, request.Page, request.Size));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateBranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBranch([FromRoute] Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateBranchRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateBranchCommand>(request);
        command.Id = id;

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<UpdateBranchResponse>
        {
            Success = true,
            Message = "Branch updated successfully",
            Data = _mapper.Map<UpdateBranchResponse>(response)
        });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranch([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBranchCommand(id), cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Branch deleted successfully" });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost("{branchId}/address")]
    [ProducesResponseType(typeof(ApiResponseWithData<BranchAddressResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranchAddress([FromRoute] Guid branchId, [FromBody] BranchAddressRequest request, CancellationToken cancellationToken)
    {
        var validator = new BranchAddressRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateBranchAddressCommand>(request);
        command.BranchId = branchId;

        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<BranchAddressResponse>
        {
            Success = true,
            Message = "Branch address created successfully",
            Data = _mapper.Map<BranchAddressResponse>(response)
        });
    }

    [Authorize]
    [HttpGet("{branchId}/address")]
    [ProducesResponseType(typeof(ApiResponseWithData<BranchAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchAddress([FromRoute] Guid branchId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetBranchAddressCommand(branchId), cancellationToken);

        return Ok(new ApiResponseWithData<BranchAddressResponse>
        {
            Success = true,
            Message = "Branch address retrieved successfully",
            Data = _mapper.Map<BranchAddressResponse>(response)
        });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{branchId}/address")]
    [ProducesResponseType(typeof(ApiResponseWithData<BranchAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBranchAddress([FromRoute] Guid branchId, [FromBody] BranchAddressRequest request, CancellationToken cancellationToken)
    {
        var validator = new BranchAddressRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateBranchAddressCommand>(request);
        command.BranchId = branchId;

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<BranchAddressResponse>
        {
            Success = true,
            Message = "Branch address updated successfully",
            Data = _mapper.Map<BranchAddressResponse>(response)
        });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("{branchId}/address")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranchAddress([FromRoute] Guid branchId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBranchAddressCommand(branchId), cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Branch address removed successfully" });
    }
}
