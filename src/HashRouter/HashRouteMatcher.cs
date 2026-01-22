using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace HashRouter;

/// <summary>
/// Matches hash paths to components with [HashRoute] attributes.
/// </summary>
internal sealed class HashRouteMatcher
{
    private readonly List<RouteEntry> _routes = new();

    /// <summary>
    /// Scans assemblies for components with [HashRoute] attributes.
    /// </summary>
    public void ScanAssemblies(IEnumerable<Assembly> assemblies)
    {
        _routes.Clear();

        foreach (var assembly in assemblies)
        {
            ScanAssembly(assembly);
        }

        // Sort routes: more specific routes first (more segments, fewer parameters)
        _routes.Sort((a, b) =>
        {
            // Prefer routes with more segments
            var segmentDiff = b.SegmentCount - a.SegmentCount;
            if (segmentDiff != 0) return segmentDiff;

            // Prefer routes with fewer parameters
            return a.ParameterCount - b.ParameterCount;
        });
    }

    private void ScanAssembly(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Handle partial loading issues
            types = ex.Types.Where(t => t != null).ToArray()!;
        }

        foreach (var type in types)
        {
            if (!typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(type))
                continue;

            var attributes = type.GetCustomAttributes<HashRouteAttribute>(inherit: false);

            foreach (var attribute in attributes)
            {
                var template = RouteTemplate.Parse(attribute.Template);
                _routes.Add(new RouteEntry
                {
                    ComponentType = type,
                    Template = template,
                    OriginalTemplate = attribute.Template,
                    SegmentCount = CountSegments(attribute.Template),
                    ParameterCount = template.ParameterNames.Count
                });
            }
        }
    }

    private static int CountSegments(string template)
    {
        if (string.IsNullOrEmpty(template))
            return 0;

        return template.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Attempts to match a hash (path + query) to a route.
    /// </summary>
    /// <param name="hash">The hash value (e.g., "/user/123?tab=profile")</param>
    /// <returns>The matched route data, or null if no match found</returns>
    public HashRouteData? Match(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            hash = "/";
        }

        // Parse path and query string
        var (path, queryParameters) = ParseHashWithQuery(hash);

        // Try to match against routes
        foreach (var route in _routes)
        {
            if (route.Template.TryMatch(path, out var routeParameters))
            {
                return new HashRouteData(
                    route.ComponentType,
                    route.OriginalTemplate,
                    hash,
                    routeParameters,
                    queryParameters
                );
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a hash value into path and query parameters.
    /// </summary>
    private static (string Path, Dictionary<string, string?> QueryParameters) ParseHashWithQuery(string hash)
    {
        var queryParameters = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // Split on ? to separate path from query string
        var questionIndex = hash.IndexOf('?');
        string path;
        string? queryString = null;

        if (questionIndex >= 0)
        {
            path = hash.Substring(0, questionIndex);
            queryString = hash.Substring(questionIndex + 1);
        }
        else
        {
            path = hash;
        }

        // Parse query string if present
        if (!string.IsNullOrEmpty(queryString))
        {
            var queryParts = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in queryParts)
            {
                var equalsIndex = part.IndexOf('=');
                if (equalsIndex >= 0)
                {
                    var key = HttpUtility.UrlDecode(part.Substring(0, equalsIndex));
                    var value = HttpUtility.UrlDecode(part.Substring(equalsIndex + 1));
                    queryParameters[key] = value;
                }
                else
                {
                    var key = HttpUtility.UrlDecode(part);
                    queryParameters[key] = null;
                }
            }
        }

        return (path, queryParameters);
    }

    /// <summary>
    /// Gets the count of registered routes.
    /// </summary>
    public int RouteCount => _routes.Count;

    private sealed class RouteEntry
    {
        public required Type ComponentType { get; init; }
        public required RouteTemplate Template { get; init; }
        public required string OriginalTemplate { get; init; }
        public required int SegmentCount { get; init; }
        public required int ParameterCount { get; init; }
    }
}
