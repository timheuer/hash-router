// HashRouter JavaScript Interop Module
// Provides hash-based navigation functionality for Blazor

let dotNetReference = null;

/**
 * Initialize the hash router with a .NET object reference for callbacks
 * @param {object} dotNetRef - DotNetObjectReference for invoking .NET methods
 */
export function initialize(dotNetRef) {
    dotNetReference = dotNetRef;
    
    // Listen for hash changes
    window.addEventListener('hashchange', handleHashChange);
    
    // Also listen for popstate for browser back/forward
    window.addEventListener('popstate', handleHashChange);
    
    // Return current hash on initialization
    return getHash();
}

/**
 * Dispose of the hash router, removing event listeners
 */
export function dispose() {
    window.removeEventListener('hashchange', handleHashChange);
    window.removeEventListener('popstate', handleHashChange);
    dotNetReference = null;
}

/**
 * Get the current hash value (without the # prefix)
 * @returns {string} The current hash path
 */
export function getHash() {
    const hash = window.location.hash;
    // Return empty string if no hash, or the hash without the # prefix
    return hash ? hash.substring(1) : '';
}

/**
 * Set the hash value programmatically
 * @param {string} hash - The hash path to navigate to (without # prefix)
 * @param {boolean} replaceState - If true, replace current history entry instead of pushing
 */
export function setHash(hash, replaceState = false) {
    const newHash = hash.startsWith('#') ? hash : '#' + hash;
    
    if (replaceState) {
        // Replace current history entry
        const url = new URL(window.location);
        url.hash = newHash;
        window.history.replaceState(null, '', url);
    } else {
        // Push new history entry
        window.location.hash = newHash;
    }
}

/**
 * Navigate to a hash without triggering the callback (for internal use)
 * @param {string} hash - The hash path to navigate to
 */
export function navigateSilent(hash, replaceState = false) {
    const oldRef = dotNetReference;
    dotNetReference = null;
    setHash(hash, replaceState);
    dotNetReference = oldRef;
}

/**
 * Handle hash change events and notify .NET
 */
function handleHashChange() {
    if (dotNetReference) {
        const hash = getHash();
        dotNetReference.invokeMethodAsync('OnHashChanged', hash);
    }
}

/**
 * Get the full URI including protocol, host, path, and hash
 * @returns {string} The full URI
 */
export function getUri() {
    return window.location.href;
}

/**
 * Get the base URI (without hash)
 * @returns {string} The base URI
 */
export function getBaseUri() {
    return window.location.origin + window.location.pathname + window.location.search;
}
