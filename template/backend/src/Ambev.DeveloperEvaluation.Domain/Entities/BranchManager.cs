using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Which branch a Manager is scoped to. One row per manager (unique on UserId) — a manager
/// manages exactly one branch today. Admin still bypasses this everywhere; this only narrows
/// what a Manager (as opposed to Admin) is allowed to touch.
/// </summary>
public class BranchManager : BaseEntity
{
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }

    public Branch? Branch { get; set; }
    public User? User { get; set; }
}
