using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.FileSync.Registrars;
using Soenneker.Utils.Template.Abstract;

namespace Soenneker.Utils.Template.Registrars;

/// <summary>
/// A powerful and extensible rendering utility
/// </summary>
public static class TemplateUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ITemplateUtil"/> as a singleton Util. <para/>
    /// </summary>
    public static IServiceCollection AddTemplateUtilAsSingleton(this IServiceCollection services)
    {
        services.AddFileUtilSyncAsScoped().AddFileUtilAsScoped().TryAddSingleton<ITemplateUtil, TemplateUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ITemplateUtil"/> as a scoped Util. <para/>
    /// </summary>
    public static IServiceCollection AddTemplateUtilAsScoped(this IServiceCollection services)
    {
        services.AddFileUtilSyncAsSingleton().AddFileUtilAsSingleton().TryAddScoped<ITemplateUtil, TemplateUtil>();

        return services;
    }
}