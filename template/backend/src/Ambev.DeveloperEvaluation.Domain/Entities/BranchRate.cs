using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class BranchRate : BaseEntity
{
    public Guid BranchId { get; set; }
    public decimal AverageRate { get; set; }
    public int ReviewCount { get; set; }

    public Branch? Branch { get; set; }
}
