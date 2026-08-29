[![](https://img.shields.io/nuget/v/soenneker.utils.template.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.template/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.template/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.template/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.template.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.template/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.template/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.template/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Template
A powerful and extensible rendering utility.

## Installation

```bash
dotnet add package Soenneker.Utils.Template
```

## Quick start

```csharp
using Soenneker.Utils.Template.Registrars;

services.AddTemplateUtilAsSingleton();
```

Then inject `ITemplateUtil` wherever you need it.

## Common operations

- `Render()` - Renders a Scriban template file with the supplied tokens and optional partials, returning the final string asynchronously.
- `RenderWithContent()` - Renders a content template first, places it in the main template's tokens under `Body` by default, then returns the final rendered string.
