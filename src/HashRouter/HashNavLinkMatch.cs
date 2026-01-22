namespace HashRouter;

/// <summary>
/// Specifies how a HashNavLink determines whether it matches the current URL.
/// </summary>
public enum HashNavLinkMatch
{
    /// <summary>
    /// The link is considered active if the current URL path starts with the link's href.
    /// </summary>
    Prefix = 0,

    /// <summary>
    /// The link is considered active only if the current URL path exactly matches the link's href.
    /// </summary>
    All = 1
}
