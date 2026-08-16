using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProductEvaluations;

public class ListProductEvaluationsHandler : IRequestHandler<ListProductEvaluationsCommand, ListProductEvaluationsResult>
{
    private readonly IProductEvaluationRepository _productEvaluationRepository;

    public ListProductEvaluationsHandler(IProductEvaluationRepository productEvaluationRepository)
    {
        _productEvaluationRepository = productEvaluationRepository;
    }

    public async Task<ListProductEvaluationsResult> Handle(ListProductEvaluationsCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListProductEvaluationsValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (evaluations, totalCount) = await _productEvaluationRepository.GetPagedAsync(
            command.Page, command.PageSize,
            filters: [x => x.ProductId == command.ProductId],
            cancellationToken: cancellationToken);

        var items = evaluations.Select(x => new ProductEvaluationItemResult
        {
            Id = x.Id,
            ProductId = x.ProductId,
            UserId = x.UserId,
            Rate = x.Rate,
            Comment = x.Comment
        });

        return new ListProductEvaluationsResult { Items = items, TotalCount = totalCount };
    }
}
