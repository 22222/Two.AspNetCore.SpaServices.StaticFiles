using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Options;
using System;
using Two.AspNetCore.SpaServices.StaticFiles;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An alternative version of <see cref="SpaStaticFilesExtensions"/> that supports rewriting URLs in static file contents.
    /// </summary>
    public static class UrlRewriteSpaStaticFilesExtensions
    {
        /// <summary>
        /// An alternative version of <see cref="SpaStaticFilesExtensions.AddSpaStaticFiles(IServiceCollection, Action{SpaStaticFilesOptions})"/> that adds support for URL rewriting.
        /// </summary>
        public static void AddSpaStaticFilesWithUrlRewrite(this IServiceCollection services, Action<UrlRewriteSpaStaticFilesOptions> configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddHttpContextAccessor();
            services.AddSingleton<ISpaStaticFileProvider>(serviceProvider =>
            {
                var optionsProvider = serviceProvider.GetService<IOptions<UrlRewriteSpaStaticFilesOptions>>();
                var options = optionsProvider.Value;
                configuration?.Invoke(options);
                if (string.IsNullOrEmpty(options.RootPath))
                {
                    throw new InvalidOperationException($"No {nameof(SpaStaticFilesOptions.RootPath)} was set on the {nameof(SpaStaticFilesOptions)}.");
                }
                return new UrlWriteSpaStaticFileProvider(serviceProvider, options);
            });
        }
    }
}
