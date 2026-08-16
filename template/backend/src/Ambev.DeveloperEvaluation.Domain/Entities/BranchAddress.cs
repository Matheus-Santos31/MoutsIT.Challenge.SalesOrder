using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class BranchAddress : BaseEntity
{
    public Guid BranchId { get; set; }
    public Guid AddressId { get; set; }

    public Branch? Branch { get; set; }
    public Address? Address { get; set; }
}
