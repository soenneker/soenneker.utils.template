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

    public async ValueTask<string> Render(string templateFilePath, Dictionary<string, object> replacements, string? contentFilePath = null,
        Dictionary<string, string>? partials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (templateFilePath.IsNullOrWhiteSpace())
                throw new ArgumentException("Template file path is required", nameof(templateFilePath));

            if (!_fileUtilSync.Exists(templateFilePath))
                throw new FileNotFoundException($"Template file not found: {templateFilePath}");

            string templateText = await _fileUtil.Read(templateFilePath, cancellationToken).NoSync();

            Scriban.Template? parsedTemplate = Scriban.Template.Parse(templateText);

            if (parsedTemplate.HasErrors)
                throw new InvalidOperationException($"Template parse errors: {string.Join(", ", parsedTemplate.Messages)}");

            var scriptObject = new ScriptObject(replacements.Count + (partials?.Count ?? 0) + 1);

            foreach (KeyValuePair<string, object> kvp in replacements)
            {
                scriptObject.SetValue(kvp.Key, kvp.Value, true);
            }

            if (!contentFilePath.IsNullOrWhiteSpace())
            {
                if (!_fileUtilSync.Exists(contentFilePath))
                    throw new FileNotFoundException($"Content file not found: {contentFilePath}");

                string contentText = await _fileUtil.Read(contentFilePath, cancellationToken).NoSync();
                Scriban.Template? contentTemplate = Scriban.Template.Parse(contentText);
                if (contentTemplate.HasErrors)
                    throw new InvalidOperationException($"Content template parse errors: {string.Join(", ", contentTemplate.Messages)}");

                var contentContext = new TemplateContext();
                contentContext.PushGlobal(scriptObject);

                string renderedContent = await contentTemplate.RenderAsync(contentContext).NoSync();
                scriptObject.SetValue("Body", renderedContent, true);
            }

            if (partials is {Count: > 0})
            {
                foreach ((string key, string value) in partials)
                {
                    scriptObject.SetValue(key, new Func<string>(() => value), true);
                }
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            return await parsedTemplate.RenderAsync(context).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template: {TemplateFilePath} / content: {ContentFilePath}", templateFilePath, contentFilePath);
            throw;
        }
    }
}