using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.FileSync.Abstract;
using Soenneker.Utils.Template.Abstract;
using Soenneker.Extensions.String;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Utils.Template;

/// <inheritdoc cref="ITemplateUtil"/>
public class TemplateUtil : ITemplateUtil
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
                throw new FileNotFoundException($"Template file not found at path: {templateFilePath}");

            string templateText = await _fileUtil.Read(templateFilePath, cancellationToken).NoSync();
            Scriban.Template? template = Scriban.Template.Parse(templateText);

            if (template.HasErrors)
                throw new InvalidOperationException($"Template parse errors: {string.Join(", ", template.Messages)}");

            var scriptObject = new ScriptObject();

            foreach (KeyValuePair<string, object> kvp in replacements)
            {
                scriptObject[kvp.Key] = kvp.Value;
            }

            // Render content.html if provided
            if (!contentFilePath.IsNullOrWhiteSpace())
            {
                if (!_fileUtilSync.Exists(contentFilePath))
                    throw new FileNotFoundException($"Content file not found at path: {contentFilePath}");

                string contentText = await _fileUtil.Read(contentFilePath, cancellationToken).NoSync();

                Scriban.Template? contentTemplate = Scriban.Template.Parse(contentText);
                if (contentTemplate.HasErrors)
                    throw new InvalidOperationException($"Content template parse errors: {string.Join(", ", contentTemplate.Messages)}");

                var contentContext = new TemplateContext();
                contentContext.PushGlobal(scriptObject); // Reuse same variables

                string renderedContent = await contentTemplate.RenderAsync(contentContext).NoSync();
                scriptObject["Body"] = renderedContent;
            }

            if (partials != null)
            {
                foreach ((string key, string value) in partials)
                {
                    // Register partial as a raw HTML-returning function
                    scriptObject[key] = new Func<string>(() => value);
                }
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            return await template.RenderAsync(context).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render Scriban template from path: {TemplateFilePath} with content path: {ContentFilePath}", templateFilePath,
                contentFilePath);
            throw;
        }
    }
}
