using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Soenneker.Extensions.String;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.FileSync.Abstract;
using Soenneker.Utils.Template.Abstract;

namespace Soenneker.Utils.Template;

/// <inheritdoc cref="ITemplateUtil"/>
public sealed class TemplateUtil : ITemplateUtil
{
    private readonly IFileUtil _fileUtil;
    private readonly ILogger<TemplateUtil> _logger;
    private readonly IFileUtilSync _fileUtilSync;

    public TemplateUtil(IFileUtil fileUtil, ILogger<TemplateUtil> logger, IFileUtilSync fileUtilSync)
    {
        _fileUtil = fileUtil;
        _logger = logger;
        _fileUtilSync = fileUtilSync;
    }

    public async ValueTask<string> Render(string templateFilePath, Dictionary<string, object> tokens, Dictionary<string, string>? partials = null,
        CancellationToken cancellationToken = default)
    {
        if (templateFilePath.IsNullOrWhiteSpace())
            throw new ArgumentException("Template file path is required", nameof(templateFilePath));

        if (!_fileUtilSync.Exists(templateFilePath))
            throw new FileNotFoundException($"Template file not found: {templateFilePath}");

        try
        {
            // Load & parse main template
            string templateText = await _fileUtil.Read(templateFilePath, true, cancellationToken).NoSync();
            Scriban.Template? parsedTemplate = Scriban.Template.Parse(templateText);
            if (parsedTemplate.HasErrors)
                throw new InvalidOperationException($"Template parse errors: {string.Join(", ", parsedTemplate.Messages)}");

            // Build a single ScriptObject that contains all tokens + partials
            var scriptObject = new ScriptObject(tokens.Count + (partials?.Count ?? 0));
            foreach (KeyValuePair<string, object> kvp in tokens)
            {
                scriptObject.SetValue(kvp.Key, kvp.Value, true);
            }

            if (partials is {Count: > 0})
            {
                foreach ((string key, string value) in partials)
                {
                    // partial can be a function that returns the raw text to inject, if you want
                    scriptObject.SetValue(key, new Func<string>(() => value), true);
                }
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            return await parsedTemplate.RenderAsync(context).NoSync();
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

        if (!_fileUtilSync.Exists(contentFilePath))
            throw new FileNotFoundException($"Content file not found: {contentFilePath}");

        // Render the “content” template into a string first
        string contentText = await _fileUtil.Read(contentFilePath, true, cancellationToken).NoSync();

        Scriban.Template? contentTemplate = Scriban.Template.Parse(contentText);
        if (contentTemplate.HasErrors)
            throw new InvalidOperationException($"Content template parse errors: {string.Join(", ", contentTemplate.Messages)}");

        // We reuse the same tokens dictionary, just insert the rendered content under the chosen key
        var contentContext = new TemplateContext();
        // Push existing tokens so contentTemplate can also use them
        var partialObject = new ScriptObject(tokens.Count);

        foreach (KeyValuePair<string, object> kvp in tokens)
        {
            partialObject.SetValue(kvp.Key, kvp.Value, true);
        }

        contentContext.PushGlobal(partialObject);

        string renderedContent = await contentTemplate.RenderAsync(contentContext).NoSync();

        // Inject it under contentPlaceholderKey
        tokens[contentPlaceholderKey] = renderedContent!;

        // Now call the core Render
        return await Render(templateFilePath, tokens, partials, cancellationToken).NoSync();
    }
}