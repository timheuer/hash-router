---
agent: Opus 4.5 Custom
---
## Plan: Blazor Hash-Based Router Component (NuGet Library)

Build a reusable Blazor class library (NuGet package) providing hash-based routing (`#/counter`, `#/user/123?tab=profile`) for client-side SPAs, with route parameter and query string support.

### Steps

1. **Create solution structure** with two projects:
   - `HashRouter` — Razor Class Library (RCL) containing the router components
   - `HashRouter.Sample` — Blazor WebAssembly standalone app for testing/demo

2. **Create JavaScript interop module** in RCL ([wwwroot/hashRouter.js](wwwroot/hashRouter.js)):
   - Listen to `hashchange` event and invoke .NET callback via `DotNetObjectReference`
   - Export `getHash()`, `setHash(hash)`, and `initialize(dotNetRef)` functions

3. **Build core routing infrastructure** in RCL:
   - `[HashRoute("...")]` attribute for defining hash routes on components
   - `HashRouteData` class to hold matched route info, parameters, and query values
   - Route template parser supporting constraints (`{id:int}`, `{name?}`)

4. **Build `HashRouter.razor` component**:
   - Scan assemblies for `[HashRoute]` attributes
   - Parse hash path and query string (e.g., `#/user/123?tab=settings`)
   - Match routes, extract parameters, render via `DynamicComponent`
   - Support `Found`, `NotFound`, `Navigating` render fragments

5. **Build `HashNavLink.razor` component**:
   - Render `<a href="#/route">` with automatic `active` class
   - Support `Match` property (`All` / `Prefix`)

6. **Create `HashNavigationManager` service**:
   - `NavigateTo(hash)`, `Uri`, `Hash`, `LocationChanged` event
   - `GetUriWithQueryParameter()` helpers for query string manipulation
   - Register via `AddHashRouting()` extension method

7. **Create `[SupplyParameterFromHashQuery]` attribute** for binding query string values to component parameters

8. **Configure sample app** (`HashRouter.Sample`):
   - Sample pages: `Home`, `Counter`, `UserDetail` (with `{id}` param and `?tab=` query)
   - Demonstrate all routing features

9. **Add NuGet packaging metadata** to RCL `.csproj`
