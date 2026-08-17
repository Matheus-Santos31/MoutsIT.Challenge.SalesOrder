using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class GetSaleResponse
{
    public Guid Id { get; set; }
    public long OrderId { get; set; }
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchDocNumber { get; set; } = string.Empty;
    public string BranchCompanyName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ProductsQuantity { get; set; }
    public int ItemsQuantity { get; set; }
    public decimal TotalDiscount { get; set; }
    public SaleStatus Status { get; set; }
    public SaleAddressResponse CustomerAddress { get; set; } = new();
    public SaleAddressResponse BranchAddress { get; set; } = new();
    public IEnumerable<SaleItemResponse> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
