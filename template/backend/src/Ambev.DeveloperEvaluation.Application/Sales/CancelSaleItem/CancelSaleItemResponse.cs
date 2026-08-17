namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemResponse
{
    public bool Success { get; set; }

    /// <summary>True when cancelling this item left no Active items, auto-cancelling the whole sale.</summary>
    public bool SaleWasCancelled { get; set; }
}
