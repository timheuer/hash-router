using Microsoft.Extensions.DependencyInjection;

namespace HashRouter;

/// <summary>
/// Extension methods for configuring hash routing services.
/// </summary>
public static class HashRoutingServiceCollectionExtensions
{
    /// <summary>
    /// Adds hash routing services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHashRouting(this IServiceCollection services)
    {
        services.AddScoped<HashNavigationManager>();
        return services;
    }
}
