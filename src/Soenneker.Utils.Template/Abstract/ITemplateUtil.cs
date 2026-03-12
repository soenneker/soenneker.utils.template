using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Template.Abstract
{
    /// <summary>
    /// Defines methods for rendering Scriban templates with optional content placeholders and partials.
    /// </summary>
    public interface ITemplateUtil
    {
        /// <summary>
        /// Renders a Scriban template at the specified <paramref name="templateFilePath"/>,
        /// using the given <paramref name="tokens"/> and optional <paramref name="partials"/>.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of the main Scriban template to render.
        /// </param>
        /// <param name="tokens">
        /// A dictionary of keys and values to supply as global variables in the template.
        /// </param>
        /// <param name="partials">
        /// An optional dictionary of named partials (each value is raw template text) to register.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to cancel the rendering operation.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with the rendered output string.
        /// </returns>
        ValueTask<string> Render(string templateFilePath, Dictionary<string, object> tokens, Dictionary<string, string>? partials = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// First renders the template at <paramref name="contentFilePath"/> into memory under the
        /// specified <paramref name="contentPlaceholderKey"/>, then renders the main template at
        /// <paramref name="templateFilePath"/>. By default, uses "Body" as the placeholder key.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of the main Scriban template to render.
        /// </param>
        /// <param name="tokens">
        /// A dictionary of keys and values to supply as global variables in both the content and main templates.
        /// </param>
        /// <param name="contentFilePath">
        /// The file path of the secondary "content" template to render first.
        /// </param>
        /// <param name="contentPlaceholderKey">
        /// The dictionary key under which the rendered content is placed before rendering the main template.
        /// Defaults to "Body".
        /// </param>
        /// <param name="partials">
        /// An optional dictionary of named partials (each value is raw template text) to register for the main template.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to cancel the entire rendering operation.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with the final rendered output string.
        /// </returns>
        ValueTask<string> RenderWithContent(string templateFilePath, Dictionary<string, object> tokens, string contentFilePath,
            string contentPlaceholderKey = "Body", Dictionary<string, string>? partials = null, CancellationToken cancellationToken = default);
    }
}