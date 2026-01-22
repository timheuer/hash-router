using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HashRouter;

/// <summary>
/// Parses and matches route templates with parameter constraints.
/// </summary>
internal sealed class RouteTemplate
{
    private static readonly Dictionary<string, Func<string, (bool Success, object? Value)>> Constraints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["int"] = TryParseInt,
        ["long"] = TryParseLong,
        ["bool"] = TryParseBool,
        ["guid"] = TryParseGuid,
        ["datetime"] = TryParseDateTime,
        ["decimal"] = TryParseDecimal,
        ["double"] = TryParseDouble,
        ["float"] = TryParseFloat,
    };

    private readonly List<TemplateSegment> _segments;

    /// <summary>
    /// Gets the original template string.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Gets the parameter names defined in this template.
    /// </summary>
    public IReadOnlyList<string> ParameterNames { get; }

    private RouteTemplate(string template, List<TemplateSegment> segments, List<string> parameterNames)
    {
        Template = template;
        _segments = segments;
        ParameterNames = parameterNames;
    }

    /// <summary>
    /// Parses a route template string into a RouteTemplate object.
    /// </summary>
    /// <param name="template">The template string (e.g., "/user/{id:int}")</param>
    /// <returns>A parsed RouteTemplate</returns>
    public static RouteTemplate Parse(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return new RouteTemplate(template, new List<TemplateSegment>(), new List<string>());
        }

        // Normalize: remove leading/trailing slashes for consistent matching
        var normalizedTemplate = template.Trim('/');

        if (string.IsNullOrEmpty(normalizedTemplate))
        {
            return new RouteTemplate(template, new List<TemplateSegment>(), new List<string>());
        }

        var parts = normalizedTemplate.Split('/');
        var segments = new List<TemplateSegment>();
        var parameterNames = new List<string>();

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            var segment = ParseSegment(part);
            segments.Add(segment);

            if (segment.IsParameter)
            {
                parameterNames.Add(segment.Name);
            }
        }

        return new RouteTemplate(template, segments, parameterNames);
    }

    private static TemplateSegment ParseSegment(string segment)
    {
        // Check if this is a parameter segment: {name}, {name:constraint}, {name?}, {name:constraint?}
        if (segment.StartsWith('{') && segment.EndsWith('}'))
        {
            var parameterContent = segment.Substring(1, segment.Length - 2);

            // Check for optional marker
            var isOptional = parameterContent.EndsWith('?');
            if (isOptional)
            {
                parameterContent = parameterContent.Substring(0, parameterContent.Length - 1);
            }

            // Check for constraint
            string? constraint = null;
            var colonIndex = parameterContent.IndexOf(':');
            if (colonIndex >= 0)
            {
                constraint = parameterContent.Substring(colonIndex + 1);
                parameterContent = parameterContent.Substring(0, colonIndex);
            }

            return new TemplateSegment
            {
                Name = parameterContent,
                IsParameter = true,
                IsOptional = isOptional,
                Constraint = constraint
            };
        }

        // Literal segment
        return new TemplateSegment
        {
            Name = segment,
            IsParameter = false,
            IsOptional = false,
            Constraint = null
        };
    }

    /// <summary>
    /// Attempts to match a path against this template.
    /// </summary>
    /// <param name="path">The path to match (e.g., "/user/123")</param>
    /// <param name="parameters">Output dictionary of extracted parameters</param>
    /// <returns>True if the path matches this template</returns>
    public bool TryMatch(string path, out Dictionary<string, object?> parameters)
    {
        parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(path))
        {
            // Empty path only matches empty template
            return _segments.Count == 0 || _segments.TrueForAll(s => s.IsOptional);
        }

        // Normalize: remove leading/trailing slashes
        var normalizedPath = path.Trim('/');

        if (string.IsNullOrEmpty(normalizedPath))
        {
            return _segments.Count == 0 || _segments.TrueForAll(s => s.IsOptional);
        }

        var pathParts = normalizedPath.Split('/');
        var pathIndex = 0;

        for (var segmentIndex = 0; segmentIndex < _segments.Count; segmentIndex++)
        {
            var segment = _segments[segmentIndex];

            if (pathIndex >= pathParts.Length)
            {
                // No more path parts
                if (segment.IsOptional)
                {
                    parameters[segment.Name] = null;
                    continue;
                }
                return false;
            }

            var pathPart = pathParts[pathIndex];

            if (segment.IsParameter)
            {
                // Try to match and convert the parameter
                if (!TryConvertParameter(pathPart, segment.Constraint, out var value))
                {
                    if (segment.IsOptional)
                    {
                        parameters[segment.Name] = null;
                        // Don't advance pathIndex for optional non-matching segments
                        continue;
                    }
                    return false;
                }

                parameters[segment.Name] = value;
                pathIndex++;
            }
            else
            {
                // Literal segment - must match exactly (case-insensitive)
                if (!string.Equals(segment.Name, pathPart, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                pathIndex++;
            }
        }

        // Check if all path parts were consumed
        return pathIndex == pathParts.Length;
    }

    private static bool TryConvertParameter(string value, string? constraint, out object? result)
    {
        result = null;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (string.IsNullOrEmpty(constraint))
        {
            // No constraint - accept as string
            result = value;
            return true;
        }

        if (Constraints.TryGetValue(constraint, out var converter))
        {
            var (success, convertedValue) = converter(value);
            if (success)
            {
                result = convertedValue;
                return true;
            }
            return false;
        }

        // Unknown constraint - treat as string
        result = value;
        return true;
    }

    private static (bool Success, object? Value) TryParseInt(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseLong(string value)
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseBool(string value)
    {
        if (bool.TryParse(value, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseGuid(string value)
    {
        if (Guid.TryParse(value, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseDateTime(string value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
            return (true, result);
        return (false, null);
    }

    private static (bool Success, object? Value) TryParseFloat(string value)
    {
        if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
            return (true, result);
        return (false, null);
    }

    private sealed class TemplateSegment
    {
        public required string Name { get; init; }
        public bool IsParameter { get; init; }
        public bool IsOptional { get; init; }
        public string? Constraint { get; init; }
    }
}
