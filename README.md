# HashRouter

A hash-based router for Blazor WebAssembly SPAs. Provides client-side routing using URL hash fragments (`#/route`) with support for route parameters, constraints, and query strings.

## Features

- **Hash-based routing**: Routes like `#/counter`, `#/user/123`
- **Route parameters**: `{id}`, `{name?}` (optional)
- **Parameter constraints**: `{id:int}`, `{guid:guid}`, etc.
- **Query string support**: `#/user/123?tab=profile`
- **Query parameter binding**: `[SupplyParameterFromHashQuery]` attribute
- **Navigation service**: `HashNavigationManager` for programmatic navigation
- **Active link detection**: `HashNavLink` component with `Match` property

## Installation

```bash
dotnet add package HashRouter
```

## Quick Start

### 1. Register services

```csharp
// Program.cs
builder.Services.AddHashRouting();
```

### 2. Use the HashRouterComponent

```razor
<!-- App.razor -->
<HashRouterComponent AppAssembly="@(new[] { typeof(App).Assembly })">
    <Found Context="routeData">
        <MainLayout>
            <DynamicComponent Type="@routeData.ComponentType" 
                              Parameters="@routeData.GetAllParameters()" />
        </MainLayout>
    </Found>
    <NotFound>
        <p>Page not found</p>
    </NotFound>
</HashRouterComponent>
```

### 3. Define routes on components

```razor
@attribute [HashRoute("/")]
<h1>Home</h1>
```

```razor
@attribute [HashRoute("/user/{Id:int}")]

<h1>User @Id</h1>

@code {
    [Parameter]
    public int Id { get; set; }
}
```

### 4. Use HashNavLink for navigation

```razor
<HashNavLink Href="/counter">Counter</HashNavLink>
<HashNavLink Href="/" Match="HashNavLinkMatch.All">Home</HashNavLink>
```

### 5. Query string parameters

```razor
@attribute [HashRoute("/search")]

<h1>Search: @Query</h1>

@code {
    [Parameter, SupplyParameterFromHashQuery(Name = "q")]
    public string? Query { get; set; }
}
```

### 6. Programmatic navigation

```razor
@inject HashNavigationManager Navigation

<button @onclick="GoToUser">Go to User</button>

@code {
    private async Task GoToUser()
    {
        await Navigation.NavigateToAsync("/user/123?tab=profile");
    }
}
```

## Route Constraints

Supported constraints:
- `{id:int}` - Integer
- `{id:long}` - Long integer
- `{flag:bool}` - Boolean
- `{id:guid}` - GUID
- `{date:datetime}` - DateTime
- `{amount:decimal}` - Decimal
- `{value:double}` - Double
- `{value:float}` - Float

## Building the NuGet Package

```powershell
dotnet pack src/HashRouter/HashRouter.csproj -c Release
```

The package will be output to `src/HashRouter/bin/Release/HashRouter.<version>.nupkg`.

To install in your project:
```powershell
dotnet add package HashRouter
```

## API Reference

### HashRouterComponent

| Parameter | Type | Description |
|-----------|------|-------------|
| `AppAssembly` | `IEnumerable<Assembly>` | Assemblies to scan for routes |
| `AdditionalAssemblies` | `IEnumerable<Assembly>` | Additional assemblies to scan |
| `Found` | `RenderFragment<HashRouteData>` | Content when route is found |
| `NotFound` | `RenderFragment` | Content when route is not found |
| `Navigating` | `RenderFragment` | Content during navigation |
| `OnRouteChanged` | `EventCallback<HashRouteData?>` | Callback when route changes |

### HashRouteData

| Property | Type | Description |
|----------|------|-------------|
| `ComponentType` | `Type` | Component type to render |
| `RouteParameters` | `IReadOnlyDictionary<string, object?>` | Parameters extracted from path |
| `QueryParameters` | `IReadOnlyDictionary<string, string?>` | Query string parameters |
| `HashPath` | `string` | Original matched hash path |
| `Template` | `string` | Matched route template |
| `GetAllParameters()` | `IDictionary<string, object?>` | Merges route + query params |

### HashNavLink Component

| Parameter | Type | Description |
|-----------|------|-------------|
| `Href` | `string` | Hash path to navigate to |
| `Class` | `string` | CSS classes |
| `ActiveClass` | `string` | CSS class when active (default: "active") |
| `Match` | `HashNavLinkMatch` | How to match (Prefix or All) |

### HashNavigationManager Service

| Property/Method | Description |
|-----------------|-------------|
| `Hash` / `Uri` | Current hash path |
| `Path` | Path portion (without query string) |
| `QueryString` | Query string portion (with `?`) |
| `LocationChanged` | Event raised on hash change |
| `NavigateTo(hash, forceLoad?, replace?)` | Navigate to hash path (sync) |
| `NavigateToAsync(hash, forceLoad?, replace?)` | Navigate asynchronously |
| `GetUriWithQueryParameter(name, value)` | Get URI with modified query param |
| `GetUriWithQueryParameters(params)` | Get URI with multiple query params |
| `TryGetQueryParameter(name, out value)` | Try to get query parameter value |
| `GetQueryParameter(name)` | Get query parameter value |

### HashLocationChangedEventArgs

| Property | Description |
|----------|-------------|
| `Location` | New hash location |
| `PreviousLocation` | Previous hash location |

## License

MIT
