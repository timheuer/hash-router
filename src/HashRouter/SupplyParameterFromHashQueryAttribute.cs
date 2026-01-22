using System;

namespace HashRouter;

/// <summary>
/// Attribute to bind a component parameter from the hash query string.
/// </summary>
/// <example>
/// [Parameter, SupplyParameterFromHashQuery]
/// public string? Tab { get; set; }
/// 
/// [Parameter, SupplyParameterFromHashQuery(Name = "q")]
/// public string? SearchQuery { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SupplyParameterFromHashQueryAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the query string parameter.
    /// If not specified, the property name is used.
    /// </summary>
    public string? Name { get; set; }
}
