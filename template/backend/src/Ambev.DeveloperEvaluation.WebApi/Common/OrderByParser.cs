namespace Ambev.DeveloperEvaluation.WebApi.Common;

public static class OrderByParser
{
    public static (string? Field, bool Ascending) Parse(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return (null, true);

        var firstToken = order.Trim('"').Split(',')[0].Trim();
        var parts = firstToken.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return (null, true);

        var ascending = parts.Length < 2 || !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (parts[0], ascending);
    }
}
