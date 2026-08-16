using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class UserAddress : BaseEntity
{
    public Guid AddressId { get; set; }
    public Guid UserId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public Address? Address { get; set; }
    public User? User { get; set; }
}
