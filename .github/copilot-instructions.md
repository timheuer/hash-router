# HashRouter - Copilot Instructions

## Project Overview

This is a **Blazor Hash-Based Router Library** that provides hash-based routing (`#/counter`, `#/user/123?tab=profile`) for Blazor WebAssembly SPAs.

## Solution Structure

```
d:\source\repos\hash-router\
├── HashRouter.slnx              # Solution file
├── README.md                    # Package documentation
├── src/
│   └── HashRouter/              # Razor Class Library (NuGet package)
│       ├── HashRouter.csproj
│       ├── wwwroot/
│       │   └── hashRouter.js    # JS interop module
│       ├── HashRouteAttribute.cs
│       ├── HashRouteData.cs
│       ├── RouteTemplate.cs     # Route parser with constraints
│       ├── HashRouteMatcher.cs  # Assembly scanning & matching
│       ├── HashRouterComponent.razor
│       ├── HashNavLink.razor
│       ├── HashNavLinkMatch.cs
│       ├── HashNavigationManager.cs
│       ├── SupplyParameterFromHashQueryAttribute.cs
│       └── HashRoutingServiceCollectionExtensions.cs
└── samples/
    └── HashRouter.Sample/       # Demo Blazor WebAssembly app
        ├── Pages/
        │   ├── Home.razor
        │   ├── Counter.razor
        │   └── UserDetail.razor  # Demonstrates params & query strings
        └── Layout/
```

## Key Components

| Component | Purpose |
|-----------|---------|
| `HashRouterComponent` | Main router - scans assemblies, matches routes, renders via DynamicComponent |
| `HashNavLink` | Navigation links with active class detection |
| `HashNavigationManager` | Service for programmatic navigation |
| `HashRouteData` | Container for matched route info (component type, route params, query params) |
| `HashLocationChangedEventArgs` | Event args for location change notifications |
| `[HashRoute("...")]` | Attribute to define routes on components |
| `[SupplyParameterFromHashQuery]` | Attribute to bind query string values |

## Route Constraints

Supports: `int`, `long`, `bool`, `guid`, `datetime`, `decimal`, `double`, `float`

Example: `[HashRoute("/user/{Id:int}")]`

Optional parameters: `[HashRoute("/search/{query?}")]`

## Implementation Details

- **Route matching**: Case-insensitive, normalizes leading/trailing slashes
- **Route priority**: More segments first, then fewer parameters
- **Parameter merging**: Route params take precedence over query params
- **JS Interop**: ES module at `./_content/HashRouter/hashRouter.js`
- **Culture**: Uses `InvariantCulture` for all parsing

## Build Commands

```powershell
cd d:\source\repos\hash-router
dotnet build
dotnet run --project samples/HashRouter.Sample
```

## NuGet Packaging

```powershell
dotnet pack src/HashRouter/HashRouter.csproj -c Release
```

Output: `src/HashRouter/bin/Release/HashRouter.<version>.nupkg`

Package metadata is configured in [src/HashRouter/HashRouter.csproj](../src/HashRouter/HashRouter.csproj).

## Important Notes

- The component is named `HashRouterComponent` (not `HashRouter`) to avoid namespace conflict
- JS module path: `./_content/HashRouter/hashRouter.js`
- Target framework: .NET 10.0
