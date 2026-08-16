using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class BranchEvaluation : BaseEntity
{
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rate { get; set; }
    public string Comment { get; set; } = string.Empty;

    public Branch? Branch { get; set; }
    public User? User { get; set; }
}
