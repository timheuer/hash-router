using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.JSInterop;

namespace HashRouter;

/// <summary>
/// Service for programmatic hash-based navigation.
/// </summary>
public sealed class HashNavigationManager : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<HashNavigationManager>? _dotNetRef;
    private bool _initialized;
    private string _currentHash = string.Empty;

    /// <summary>
    /// Event raised when the hash location changes.
    /// </summary>
    public event EventHandler<HashLocationChangedEventArgs>? LocationChanged;

    /// <summary>
    /// Gets the current hash path (without the # prefix).
    /// </summary>
    public string Hash => _currentHash;

    /// <summary>
    /// Gets the current hash path and query string.
    /// </summary>
    public string Uri => _currentHash;

    /// <summary>
    /// Gets just the path portion of the current hash (without query string).
    /// </summary>
    public string Path
    {
        get
        {
            var queryIndex = _currentHash.IndexOf('?');
            return queryIndex >= 0
                ? _currentHash.Substring(0, queryIndex)
                : _currentHash;
        }
    }

    /// <summary>
    /// Gets the query string portion of the current hash (including the ?).
    /// </summary>
    public string? QueryString
    {
        get
        {
            var queryIndex = _currentHash.IndexOf('?');
            return queryIndex >= 0
                ? _currentHash.Substring(queryIndex)
                : null;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashNavigationManager"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for interop.</param>
    public HashNavigationManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Ensures the navigation manager is initialized.
    /// Called automatically by the routing components.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        _dotNetRef = DotNetObjectReference.Create(this);
        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/HashRouter/hashRouter.js");

        _currentHash = await _jsModule.InvokeAsync<string>("initialize", _dotNetRef);
        _initialized = true;
    }

    /// <summary>
    /// Navigates to the specified hash path.
    /// </summary>
    /// <param name="hash">The hash path to navigate to (without # prefix)</param>
    /// <param name="forceLoad">If true, forces a page reload (not typically used for hash navigation)</param>
    /// <param name="replace">If true, replaces the current history entry instead of pushing</param>
    public async Task NavigateToAsync(string hash, bool forceLoad = false, bool replace = false)
    {
        await EnsureInitializedAsync();

        if (_jsModule == null)
            throw new InvalidOperationException("HashNavigationManager not initialized");

        var normalizedHash = hash.TrimStart('#');
        await _jsModule.InvokeVoidAsync("setHash", normalizedHash, replace);
    }

    /// <summary>
    /// Navigates to the specified hash path (synchronous overload for compatibility).
    /// </summary>
    public void NavigateTo(string hash, bool forceLoad = false, bool replace = false)
    {
        _ = NavigateToAsync(hash, forceLoad, replace);
    }

    /// <summary>
    /// Gets the URI with a query parameter added or updated.
    /// </summary>
    public string GetUriWithQueryParameter(string name, string? value)
    {
        return GetUriWithQueryParameters(new Dictionary<string, object?> { [name] = value });
    }

    /// <summary>
    /// Gets the URI with multiple query parameters added or updated.
    /// </summary>
    public string GetUriWithQueryParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        var path = Path;
        var existingParams = ParseQueryString(QueryString);

        foreach (var kvp in parameters)
        {
            if (kvp.Value == null)
            {
                existingParams.Remove(kvp.Key);
            }
            else
            {
                existingParams[kvp.Key] = kvp.Value.ToString();
            }
        }

        if (existingParams.Count == 0)
            return path;

        var queryParts = new List<string>();
        foreach (var kvp in existingParams)
        {
            if (kvp.Value != null)
            {
                queryParts.Add($"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}");
            }
            else
            {
                queryParts.Add(HttpUtility.UrlEncode(kvp.Key));
            }
        }

        return $"{path}?{string.Join("&", queryParts)}";
    }

    /// <summary>
    /// Tries to get a query parameter value from the current hash.
    /// </summary>
    public bool TryGetQueryParameter(string name, out string? value)
    {
        var queryParams = ParseQueryString(QueryString);
        return queryParams.TryGetValue(name, out value);
    }

    /// <summary>
    /// Gets a query parameter value from the current hash.
    /// </summary>
    public string? GetQueryParameter(string name)
    {
        TryGetQueryParameter(name, out var value);
        return value;
    }

    private static Dictionary<string, string?> ParseQueryString(string? queryString)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(queryString))
            return result;

        var query = queryString.TrimStart('?');
        var parts = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var equalsIndex = part.IndexOf('=');
            if (equalsIndex >= 0)
            {
                var key = HttpUtility.UrlDecode(part.Substring(0, equalsIndex));
                var value = HttpUtility.UrlDecode(part.Substring(equalsIndex + 1));
                result[key] = value;
            }
            else
            {
                var key = HttpUtility.UrlDecode(part);
                result[key] = null;
            }
        }

        return result;
    }

    /// <summary>
    /// Called from JavaScript when the hash changes.
    /// </summary>
    [JSInvokable]
    public Task OnHashChanged(string hash)
    {
        var oldHash = _currentHash;
        _currentHash = hash;

        LocationChanged?.Invoke(this, new HashLocationChangedEventArgs(hash, oldHash));

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignore - circuit already disconnected
            }
        }

        _dotNetRef?.Dispose();
    }
}

/// <summary>
/// Event arguments for hash location changes.
/// </summary>
public sealed class HashLocationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new hash location.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Gets the previous hash location.
    /// </summary>
    public string PreviousLocation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashLocationChangedEventArgs"/> class.
    /// </summary>
    /// <param name="location">The new hash location.</param>
    /// <param name="previousLocation">The previous hash location.</param>
    public HashLocationChangedEventArgs(string location, string previousLocation)
    {
        Location = location;
        PreviousLocation = previousLocation;
    }
}
