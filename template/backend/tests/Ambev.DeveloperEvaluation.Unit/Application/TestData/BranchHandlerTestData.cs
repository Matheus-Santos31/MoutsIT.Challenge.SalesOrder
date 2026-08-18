using Ambev.DeveloperEvaluation.Application.Branches.CreateBranch;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides Bogus-based generators for Branch-related handler commands.
/// </summary>
public static class BranchHandlerTestData
{
    private static readonly Faker<CreateBranchCommand> createBranchFaker = new Faker<CreateBranchCommand>()
        .RuleFor(x => x.Name, f => f.Company.CompanyName())
        .RuleFor(x => x.DocNumber, f => f.Random.Replace("##########0001"))
        .RuleFor(x => x.CompanyName, f => f.Company.CompanyName());

    public static CreateBranchCommand GenerateValidCreateCommand() => createBranchFaker.Generate();
}
