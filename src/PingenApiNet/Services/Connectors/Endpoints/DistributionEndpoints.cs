namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for distribution service
/// </summary>
internal static class DistributionEndpoints
{
    /// <summary>
    /// Root path of distributions
    /// </summary>
    internal const string Root = "distribution";

    /// <summary>
    /// Endpoint to get delivery products (page)
    /// </summary>
    internal const string DeliveryProducts = $"{Root}/delivery-products";
}
