using System;
using System.Collections.Generic;

namespace HashRouter;

/// <summary>
/// Contains information about a matched hash route.
/// </summary>
public sealed class HashRouteData
{
    /// <summary>
    /// Gets the component type to render.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Gets the route parameters extracted from the path.
    /// </summary>
    public IReadOnlyDictionary<string, object?> RouteParameters { get; }

    /// <summary>
    /// Gets the query string parameters.
    /// </summary>
    public IReadOnlyDictionary<string, string?> QueryParameters { get; }

    /// <summary>
    /// Gets the original hash path that was matched.
    /// </summary>
    public string HashPath { get; }

    /// <summary>
    /// Gets the matched route template.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Creates a new HashRouteData instance.
    /// </summary>
    public HashRouteData(
        Type componentType,
        string template,
        string hashPath,
        IReadOnlyDictionary<string, object?>? routeParameters = null,
        IReadOnlyDictionary<string, string?>? queryParameters = null)
    {
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Template = template ?? throw new ArgumentNullException(nameof(template));
        HashPath = hashPath ?? throw new ArgumentNullException(nameof(hashPath));
        RouteParameters = routeParameters ?? new Dictionary<string, object?>();
        QueryParameters = queryParameters ?? new Dictionary<string, string?>();
    }

    /// <summary>
    /// Gets all parameters (route + query) as a combined dictionary for DynamicComponent.
    /// </summary>
    public IDictionary<string, object?> GetAllParameters()
    {
        var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in RouteParameters)
        {
            parameters[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in QueryParameters)
        {
            // Only add query param if not already present from route
            if (!parameters.ContainsKey(kvp.Key))
            {
                parameters[kvp.Key] = kvp.Value;
            }
        }

        return parameters;
    }
}
