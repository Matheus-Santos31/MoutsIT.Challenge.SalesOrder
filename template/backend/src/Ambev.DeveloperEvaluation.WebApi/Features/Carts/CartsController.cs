using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.GetCart;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.ListCarts;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.AddCartItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCartItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.CartItem;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Carts.CancelCart;
using Ambev.DeveloperEvaluation.Application.Carts.AddCartItem;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCartItem;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCartItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

[ApiController]
[Route("api/[controller]")]
public class CartsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CartsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateCartResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateCartRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateCartCommand>(request);
        command.UserId = GetCurrentUserId();

        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateCartResponse>
        {
            Success = true,
            Message = "Cart created successfully",
            Data = _mapper.Map<CreateCartResponse>(response)
        });
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetCartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new GetCartCommand
        {
            Id = id,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<GetCartResponse>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = _mapper.Map<GetCartResponse>(response)
        });
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CartListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCarts([FromQuery] ListCartsRequest request, CancellationToken cancellationToken)
    {
        var command = new ListCartsCommand
        {
            Page = request.Page,
            PageSize = request.Size,
            BranchId = request.BranchId,
            Status = request.Status,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        var result = await _mediator.Send(command, cancellationToken);
        var items = _mapper.Map<List<CartListItemResponse>>(result.Items);

        return OkPaginated(new PaginatedList<CartListItemResponse>(items, result.TotalCount, request.Page, request.Size));
    }

    /// <summary>
    /// Cancels a cart still open for shopping. Completing a cart is not done here —
    /// see POST /sales, which promotes the cart to Completed and creates the Sale.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelCartCommand
        {
            Id = id,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Cart cancelled successfully" });
    }

    [Authorize]
    [HttpPost("{cartId}/items")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddCartItem([FromRoute] Guid cartId, [FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var validator = new AddCartItemRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<AddCartItemCommand>(request);
        command.CartId = cartId;
        command.RequestingUserId = GetCurrentUserId();
        command.IsRequestingUserAdmin = User.IsInRole("Admin");

        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CartItemResponse>
        {
            Success = true,
            Message = "Item added successfully",
            Data = _mapper.Map<CartItemResponse>(response)
        });
    }

    [Authorize]
    [HttpPut("{cartId}/items/{itemId}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCartItem([FromRoute] Guid cartId, [FromRoute] Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateCartItemRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateCartItemCommand>(request);
        command.CartId = cartId;
        command.ItemId = itemId;
        command.RequestingUserId = GetCurrentUserId();
        command.IsRequestingUserAdmin = User.IsInRole("Admin");

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<CartItemResponse>
        {
            Success = true,
            Message = "Item updated successfully",
            Data = _mapper.Map<CartItemResponse>(response)
        });
    }

    [Authorize]
    [HttpDelete("{cartId}/items/{itemId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCartItem([FromRoute] Guid cartId, [FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        var command = new DeleteCartItemCommand
        {
            CartId = cartId,
            ItemId = itemId,
            RequestingUserId = GetCurrentUserId(),
            IsRequestingUserAdmin = User.IsInRole("Admin")
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Item removed successfully" });
    }
}
