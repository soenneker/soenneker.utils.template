[![](https://img.shields.io/nuget/v/soenneker.utils.template.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.template/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.template/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.template/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.template.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.template/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.template/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.template/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Template
Asynchronous Scriban rendering for file-based templates, with token globals, composed content, and timestamp-based parsing cache.

## Installation

```bash
dotnet add package Soenneker.Utils.Template
```

## Registration

```csharp
using Soenneker.Utils.Template.Registrars;

services.AddTemplateUtilAsSingleton();
```

The singleton registration shares parsed templates across the application. Use `AddTemplateUtilAsScoped()` when the cache should live only for a dependency-injection scope.

## Render a template

```csharp
using Soenneker.Utils.Template.Abstract;

var tokens = new Dictionary<string, object>
{
    ["Name"] = "Ada",
    ["AccountUrl"] = "https://example.test/account"
};

string result = await templateUtil.Render(
    "Templates/welcome.scriban",
    tokens,
    cancellationToken: cancellationToken);
```

Template variables use the dictionary keys directly:

```scriban
Hello {{ Name }}. Manage your account at {{ AccountUrl }}.
```

The supplied dictionary is read while globals are constructed and is not mutated. Scriban performs the value formatting; this utility does not HTML-encode output automatically.

## Compose content into a layout

`RenderWithContent` renders the content file first using the supplied tokens, then exposes the resulting string to the main template under `Body` by default:

```csharp
string page = await templateUtil.RenderWithContent(
    templateFilePath: "Templates/layout.scriban",
    tokens: tokens,
    contentFilePath: "Templates/welcome-content.scriban",
    contentPlaceholderKey: "Body",
    cancellationToken: cancellationToken);
```

```scriban
<main>{{ Body }}</main>
```

The original token dictionary is not changed. If it already contains the placeholder key, the rendered content is the value used by the main template.

## Raw partial values

The optional `partials` dictionary adds each entry as a global string. It does not parse or register Scriban include files. A value can be emitted with `{{ Footer }}`, but Scriban syntax inside that string is not rendered a second time.

## Caching and failures

Parsed templates are cached by their full file path and last-modified timestamp. A changed timestamp causes the file to be read and parsed again. With scoped registration, each scope has its own cache; with singleton registration, the cache is application-wide.

Missing files, Scriban parse errors, rendering failures, and cancellation are propagated to the caller. Rendering failures from the main template are also logged. The cancellation token applies to file access and Scriban execution.
