namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleHistory;

public class GetSaleHistoryResponse
{
    public Guid SaleId { get; set; }
    public long OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ProductsQuantity { get; set; }
    public int ItemsQuantity { get; set; }
    public decimal TotalDiscount { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<GetSaleHistoryItemResponse> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class GetSaleHistoryItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
