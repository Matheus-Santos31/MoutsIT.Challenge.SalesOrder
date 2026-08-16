using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranches;

public class ListBranchesHandler : IRequestHandler<ListBranchesCommand, ListBranchesResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchRateRepository _branchRateRepository;

    public ListBranchesHandler(IBranchRepository branchRepository, IBranchRateRepository branchRateRepository)
    {
        _branchRepository = branchRepository;
        _branchRateRepository = branchRateRepository;
    }

    public async Task<ListBranchesResult> Handle(ListBranchesCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListBranchesValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (branches, totalCount) = await _branchRepository.GetPagedAsync(
            command.Page, command.PageSize, orderBy: command.OrderBy, ascending: command.Ascending, cancellationToken: cancellationToken);

        var branchList = branches.ToList();
        var branchIds = branchList.Select(x => x.Id).ToList();

        var rates = await _branchRateRepository.GetAsync(x => branchIds.Contains(x.BranchId), cancellationToken);
        var ratesByBranch = rates.ToDictionary(x => x.BranchId);

        var items = branchList.Select(branch =>
        {
            ratesByBranch.TryGetValue(branch.Id, out var rate);
            return new BranchListItemResult
            {
                Id = branch.Id,
                Name = branch.Name,
                DocNumber = branch.DocNumber,
                CompanyName = branch.CompanyName,
                AverageRate = rate?.AverageRate,
                ReviewCount = rate?.ReviewCount
            };
        });

        return new ListBranchesResult { Items = items, TotalCount = totalCount };
    }
}
