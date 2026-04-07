namespace CoreBooking.Domain.Adapters;

public interface IExternalProviderAdapter
{
    string AdapterKey { get; }

    Task<IReadOnlyCollection<ExternalProductImportData>> ImportProductsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExternalProductContentImportData>> ImportProductContentAsync(CancellationToken cancellationToken = default);

    Task<ExternalProductAvailabilityResult> CheckAvailabilityAsync(
        string externalProductId,
        int requestedQuantity,
        CancellationToken cancellationToken = default);

    Task<string> CheckoutAsync(ExternalCheckoutRequest request, CancellationToken cancellationToken = default);
}

public sealed record ExternalProductImportData(
    string ExternalProductId,
    string Name,
    decimal Price,
    string CategoryName);

public sealed record ExternalProductContentImportData(
    string ExternalProductId,
    string ContentType,
    string Data);

public sealed record ExternalProductAvailabilityResult(
    string ExternalProductId,
    bool IsAvailable,
    int AvailableQuantity,
    string? Message = null);

public sealed record ExternalCheckoutItem(string ExternalProductId, int Quantity, decimal UnitPrice);

public sealed record ExternalCheckoutRequest(
    string UserReference,
    IReadOnlyCollection<ExternalCheckoutItem> Items,
    decimal TotalAmount);
