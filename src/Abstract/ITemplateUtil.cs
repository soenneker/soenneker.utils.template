using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Template.Abstract;

/// <summary>
/// A powerful and extensible rendering utility
/// </summary>
public interface ITemplateUtil
{
    /// <summary>
    /// Renders a Scriban-based template with replacements, optional content, and optional partials.
    /// </summary>
    /// <param name="templateFilePath">Full path to the main template file</param>
    /// <param name="replacements">Token dictionary to be applied</param>
    /// <param name="contentFilePath">Optional full path to a body/content file (injected as 'body')</param>
    /// <param name="partials">Optional dictionary of named partial templates</param>
    /// <param name="cancellationToken"></param>
    ValueTask<string> Render(string templateFilePath, Dictionary<string, object> replacements, string? contentFilePath = null,
        Dictionary<string, string>? partials = null, CancellationToken cancellationToken = default);
}