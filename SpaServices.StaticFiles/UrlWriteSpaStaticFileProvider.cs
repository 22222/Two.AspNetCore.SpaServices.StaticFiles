using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// An alternative implementation of <see cref="DefaultSpaStaticFileProvider"/> that can rewrite URLs in the static files.
    /// </summary>
    public sealed class UrlWriteSpaStaticFileProvider : ISpaStaticFileProvider, IDisposable
    {
        private readonly UrlRewriteFileProvider? fileProvider;

        public UrlWriteSpaStaticFileProvider(IServiceProvider serviceProvider, UrlRewriteSpaStaticFilesOptions options)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(options.RootPath))
            {
                throw new ArgumentException($"The {nameof(options.RootPath)} property of {nameof(options)} cannot be null or empty.", paramName: nameof(options));
            }

            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

            var absoluteRootPath = Path.Combine(env.ContentRootPath, options.RootPath);
            if (Directory.Exists(absoluteRootPath))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var physicalFileProvider = new PhysicalFileProvider(absoluteRootPath);

#pragma warning restore CA2000 // Dispose objects before losing scope
                try
                {
                    this.fileProvider = new UrlRewriteFileProvider(physicalFileProvider, httpContextAccessor, options);
                }
                catch (Exception)
                {
                    physicalFileProvider.Dispose();
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public IFileProvider? FileProvider => fileProvider;

        /// <inheritdoc />
        public void Dispose()
        {
            fileProvider?.Dispose();
        }
    }
}
