using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleResult
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
    public SaleAddressResult CustomerAddress { get; set; } = new();
    public SaleAddressResult BranchAddress { get; set; } = new();
    public IEnumerable<SaleItemResult> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
