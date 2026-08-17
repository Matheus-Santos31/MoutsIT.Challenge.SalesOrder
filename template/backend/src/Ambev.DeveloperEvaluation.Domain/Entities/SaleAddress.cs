namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Snapshot of an <see cref="Address"/> at the moment a <see cref="Sale"/> was created.
/// Owned by <see cref="Sale"/> (no identity, no FK to Address) so a later edit to the
/// customer's or branch's registered address never changes an already-completed sale.
/// </summary>
public class SaleAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;

    public static SaleAddress From(Address address) => new()
    {
        City = address.City,
        Street = address.Street,
        Number = address.Number,
        PostalCode = address.PostalCode,
        Latitude = address.Latitude,
        Longitude = address.Longitude
    };
}
