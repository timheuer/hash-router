using System;

namespace HashRouter;

/// <summary>
/// Attribute to define a hash-based route for a Blazor component.
/// </summary>
/// <example>
/// [HashRoute("/user/{id:int}")]
/// public partial class UserDetail : ComponentBase { }
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class HashRouteAttribute : Attribute
{
    /// <summary>
    /// Gets the route template.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Creates a new hash route attribute with the specified template.
    /// </summary>
    /// <param name="template">
    /// The route template (e.g., "/user/{id:int}", "/products/{category?}").
    /// Supports parameter constraints like :int, :guid, :bool, :datetime, :decimal, :double, :float, :long.
    /// Use ? suffix for optional parameters.
    /// </param>
    public HashRouteAttribute(string template)
    {
        Template = template ?? throw new ArgumentNullException(nameof(template));
    }
}
