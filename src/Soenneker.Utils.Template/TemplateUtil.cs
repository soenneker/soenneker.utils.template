using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Template.Abstract;

namespace Soenneker.Utils.Template;

/// <inheritdoc cref="ITemplateUtil" />
public sealed class TemplateUtil : ITemplateUtil
{
    private readonly IFileUtil _fileUtil;
    private readonly ILogger<TemplateUtil> _logger;
    private readonly ConcurrentDictionary<string, CachedTemplate> _templateCache = new(StringComparer.Ordinal);

    public TemplateUtil(IFileUtil fileUtil, ILogger<TemplateUtil> logger)
    {
        _fileUtil = fileUtil;
        _logger = logger;
    }

    public async ValueTask<string> Render(string templateFilePath, Dictionary<string, object> tokens, Dictionary<string, string>? partials = null,
        CancellationToken cancellationToken = default)
    {
        if (templateFilePath.IsNullOrWhiteSpace())
            throw new ArgumentException("Template file path is required", nameof(templateFilePath));

        try
        {
            Scriban.Template parsedTemplate = await GetTemplate(templateFilePath, "Template", cancellationToken).NoSync();

            ScriptObject globals = BuildGlobals(tokens, partials);

            var context = new TemplateContext
            {
                CancellationToken = cancellationToken
            };
            context.PushGlobal(globals);

            return await parsedTemplate.RenderAsync(context)
                                       .NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template: {TemplateFilePath}", templateFilePath);
            throw;
        }
    }

    public async ValueTask<string> RenderWithContent(string templateFilePath, Dictionary<string, object> tokens, string contentFilePath,
        string contentPlaceholderKey = "Body", Dictionary<string, string>? partials = null, CancellationToken cancellationToken = default)
    {
        if (contentFilePath.IsNullOrWhiteSpace())
            throw new ArgumentException("Content file path is required", nameof(contentFilePath));

        // Build globals once (tokens + partials), then render content into a string,
        // then render the main template with an augmented globals object (no mutation of tokens).
        ScriptObject baseGlobals = BuildGlobals(tokens, partials);

        Scriban.Template contentTemplate = await GetTemplate(contentFilePath, "Content template", cancellationToken).NoSync();

        var contentContext = new TemplateContext
        {
            CancellationToken = cancellationToken
        };
        contentContext.PushGlobal(baseGlobals);

        string renderedContent = await contentTemplate.RenderAsync(contentContext)
                                                      .NoSync();

        // Augment globals with the rendered content
        var finalGlobals = new ScriptObject(baseGlobals.Count + 1);
        finalGlobals.Import(baseGlobals, renamer: null, filter: null);
        finalGlobals.SetValue(contentPlaceholderKey, renderedContent, readOnly: true);

        // Render the main template
        return await RenderWithGlobals(templateFilePath, finalGlobals, cancellationToken)
            .NoSync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ScriptObject BuildGlobals(Dictionary<string, object> tokens, Dictionary<string, string>? partials)
    {
        int capacity = tokens.Count + (partials?.Count ?? 0);
        var scriptObject = new ScriptObject(capacity);

        foreach (KeyValuePair<string, object> kvp in tokens)
            scriptObject.SetValue(kvp.Key, kvp.Value, readOnly: true);

        if (partials is { Count: > 0 })
        {
            foreach (KeyValuePair<string, string> kvp in partials)
            {
                // Avoid closure allocation: store raw string
                scriptObject.SetValue(kvp.Key, kvp.Value, readOnly: true);
            }
        }

        return scriptObject;
    }

    private async ValueTask<string> RenderWithGlobals(string templateFilePath, ScriptObject globals, CancellationToken cancellationToken)
    {
        try
        {
            Scriban.Template parsedTemplate = await GetTemplate(templateFilePath, "Template", cancellationToken).NoSync();

            var context = new TemplateContext
            {
                CancellationToken = cancellationToken
            };
            context.PushGlobal(globals);

            return await parsedTemplate.RenderAsync(context)
                                       .NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template: {TemplateFilePath}", templateFilePath);
            throw;
        }
    }

    private async ValueTask<Scriban.Template> GetTemplate(string path, string description, CancellationToken cancellationToken)
    {
        DateTimeOffset? lastModified = await _fileUtil.GetLastModified(path, cancellationToken).NoSync();
        if (lastModified is null)
            throw new FileNotFoundException($"{description} file not found: {path}");

        string cacheKey = Path.GetFullPath(path);
        if (_templateCache.TryGetValue(cacheKey, out CachedTemplate cached) && cached.LastModified == lastModified.Value)
            return cached.Template;

        string text = await _fileUtil.Read(path, true, cancellationToken).NoSync();
        Scriban.Template parsed = Scriban.Template.Parse(text);
        if (parsed.HasErrors)
            throw new InvalidOperationException($"{description} parse errors: {string.Join(", ", parsed.Messages)}");

        _templateCache[cacheKey] = new CachedTemplate(lastModified.Value, parsed);
        return parsed;
    }
}
