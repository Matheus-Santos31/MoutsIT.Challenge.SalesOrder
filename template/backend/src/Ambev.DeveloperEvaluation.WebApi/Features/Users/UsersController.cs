using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.DeleteUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUserAddress;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUserAddresses;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUserAddress;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.SetDefaultUserAddress;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.CreateUserAddress;
using Ambev.DeveloperEvaluation.Application.Users.ListUserAddresses;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUserAddress;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUserAddress;
using Ambev.DeveloperEvaluation.Application.Users.SetDefaultUserAddress;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

/// <summary>
/// Controller for managing user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of UsersController
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public UsersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">The user creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateUserCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateUserResponse>
        {
            Success = true,
            Message = "User created successfully",
            Data = _mapper.Map<CreateUserResponse>(response)
        });
    }

    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetUserRequest { Id = id };
        var validator = new GetUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<GetUserCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<GetUserResponse>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = _mapper.Map<GetUserResponse>(response)
        });
    }

    /// <summary>
    /// Deletes a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response if the user was deleted</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteUserRequest { Id = id };
        var validator = new DeleteUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<DeleteUserCommand>(request.Id);
        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "User deleted successfully"
        });
    }

    /// <summary>
    /// Adds a new address for a user
    /// </summary>
    [Authorize]
    [HttpPost("{userId}/addresses")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateUserAddressResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUserAddress([FromRoute] Guid userId, [FromBody] CreateUserAddressRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserAddressRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateUserAddressCommand>(request);
        command.UserId = userId;
        command.RequestingUserId = GetCurrentUserId();
        command.IsRequestingUserAdmin = User.IsInRole("Admin");

        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateUserAddressResponse>
        {
            Success = true,
            Message = "Address added successfully",
            Data = _mapper.Map<CreateUserAddressResponse>(response)
        });
    }

    /// <summary>
    /// Lists the addresses of a user
    /// </summary>
    [Authorize]
    [HttpGet("{userId}/addresses")]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<ListUserAddressesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListUserAddresses([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var command = new ListUserAddressesCommand
        {
            UserId = userId,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<IEnumerable<ListUserAddressesResponse>>(response));
    }

    /// <summary>
    /// Updates an address of a user
    /// </summary>
    [Authorize]
    [HttpPut("{userId}/addresses/{addressId}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateUserAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserAddress([FromRoute] Guid userId, [FromRoute] Guid addressId, [FromBody] UpdateUserAddressRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateUserAddressRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateUserAddressCommand>(request);
        command.UserId = userId;
        command.AddressId = addressId;
        command.RequestingUserId = GetCurrentUserId();
        command.IsRequestingUserAdmin = User.IsInRole("Admin");

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<UpdateUserAddressResponse>
        {
            Success = true,
            Message = "Address updated successfully",
            Data = _mapper.Map<UpdateUserAddressResponse>(response)
        });
    }

    /// <summary>
    /// Removes an address of a user
    /// </summary>
    [Authorize]
    [HttpDelete("{userId}/addresses/{addressId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserAddress([FromRoute] Guid userId, [FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        var command = new Application.Users.DeleteUserAddress.DeleteUserAddressCommand
        {
            UserId = userId,
            AddressId = addressId,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Address removed successfully"
        });
    }

    /// <summary>
    /// Marks an address as the default one for a user
    /// </summary>
    [Authorize]
    [HttpPatch("{userId}/addresses/{addressId}/default")]
    [ProducesResponseType(typeof(ApiResponseWithData<SetDefaultUserAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultUserAddress([FromRoute] Guid userId, [FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        var command = new SetDefaultUserAddressCommand
        {
            UserId = userId,
            AddressId = addressId,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<SetDefaultUserAddressResponse>
        {
            Success = true,
            Message = "Default address updated successfully",
            Data = _mapper.Map<SetDefaultUserAddressResponse>(response)
        });
    }
}
