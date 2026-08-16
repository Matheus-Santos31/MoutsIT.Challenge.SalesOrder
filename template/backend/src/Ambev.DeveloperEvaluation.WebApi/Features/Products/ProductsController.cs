using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.CreateProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.GetProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.ListProducts;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.EvaluateProduct;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.ListProductEvaluations;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.EvaluateProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProductEvaluation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateProductCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateProductResponse>
        {
            Success = true,
            Message = "Product created successfully",
            Data = _mapper.Map<CreateProductResponse>(response)
        });
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetProductCommand(id), cancellationToken);

        return Ok(new ApiResponseWithData<GetProductResponse>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = _mapper.Map<GetProductResponse>(response)
        });
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts([FromQuery] ListProductsRequest request, CancellationToken cancellationToken)
    {
        var (orderField, ascending) = OrderByParser.Parse(request.Order);

        var command = new ListProductsCommand
        {
            Page = request.Page,
            PageSize = request.Size,
            OrderBy = orderField,
            Ascending = ascending,
            Category = request.Category
        };

        var result = await _mediator.Send(command, cancellationToken);
        var items = _mapper.Map<List<ProductListItemResponse>>(result.Items);

        return OkPaginated(new PaginatedList<ProductListItemResponse>(items, result.TotalCount, request.Page, request.Size));
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateProductCommand>(request);
        command.Id = id;

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<UpdateProductResponse>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = _mapper.Map<UpdateProductResponse>(response)
        });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Product deleted successfully" });
    }

    [Authorize]
    [HttpPost("{productId}/evaluations")]
    [ProducesResponseType(typeof(ApiResponseWithData<EvaluateProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EvaluateProduct([FromRoute] Guid productId, [FromBody] EvaluateProductRequest request, CancellationToken cancellationToken)
    {
        var validator = new EvaluateProductRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<EvaluateProductCommand>(request);
        command.ProductId = productId;
        command.UserId = GetCurrentUserId();

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<EvaluateProductResponse>
        {
            Success = true,
            Message = "Product evaluated successfully",
            Data = _mapper.Map<EvaluateProductResponse>(response)
        });
    }

    [Authorize]
    [HttpGet("{productId}/evaluations")]
    [ProducesResponseType(typeof(PaginatedResponse<ProductEvaluationItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProductEvaluations([FromRoute] Guid productId, [FromQuery] ListProductEvaluationsRequest request, CancellationToken cancellationToken)
    {
        var command = new Application.Products.ListProductEvaluations.ListProductEvaluationsCommand
        {
            ProductId = productId,
            Page = request.Page,
            PageSize = request.Size
        };

        var result = await _mediator.Send(command, cancellationToken);
        var items = _mapper.Map<List<ProductEvaluationItemResponse>>(result.Items);

        return OkPaginated(new PaginatedList<ProductEvaluationItemResponse>(items, result.TotalCount, request.Page, request.Size));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId}/evaluations/{evaluationId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductEvaluation([FromRoute] Guid productId, [FromRoute] Guid evaluationId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductEvaluationCommand(productId, evaluationId), cancellationToken);

        return Ok(new ApiResponse { Success = true, Message = "Evaluation removed successfully" });
    }
}
