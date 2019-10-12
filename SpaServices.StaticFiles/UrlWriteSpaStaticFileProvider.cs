﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// An alternative implementation of <see cref="DefaultSpaStaticFileProvider"/> that can rewrite URLs in the static files.
    /// </summary>
    public sealed class UrlWriteSpaStaticFileProvider : ISpaStaticFileProvider, IDisposable
    {
        private readonly UrlRewriteFileProvider fileProvider;

        public UrlWriteSpaStaticFileProvider(IServiceProvider serviceProvider, UrlRewriteSpaStaticFilesOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(options.RootPath))
            {
                throw new ArgumentException($"The {nameof(options.RootPath)} property of {nameof(options)} cannot be null or empty.");
            }

            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

            var absoluteRootPath = Path.Combine(env.ContentRootPath, options.RootPath);
            if (Directory.Exists(absoluteRootPath))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var physicalFileProvider = new PhysicalFileProvider(absoluteRootPath);
#pragma warning restore CA2000 // Dispose objects before losing scope
                this.fileProvider = new UrlRewriteFileProvider(physicalFileProvider, httpContextAccessor, options);
            }
        }

        /// <inheritdoc />
        public IFileProvider FileProvider => fileProvider;

        /// <inheritdoc />
        public void Dispose()
        {
            fileProvider.Dispose();
        }
    }

    /// <summary>
    /// An implementation of <see cref="IFileProvider"/> that can rewrite URLs in the file content.
    /// </summary>
    internal sealed class UrlRewriteFileProvider : IFileProvider, IDisposable
    {
        private readonly IFileProvider innerFileProvider;
        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IReadOnlyCollection<ISpaStaticFilesUrlRewriter> rewriters;
        private readonly string sourcePathBase;
        private readonly Func<HttpRequest, string> sourcePathBaseSelector;
        private readonly Func<HttpRequest, string> targetPathBaseSelector;
        private readonly int? maxFileLengthForRewrite;

        internal UrlRewriteFileProvider(IFileProvider innerFileProvider, IHttpContextAccessor httpContextAccessor, UrlRewriteSpaStaticFilesOptions options)
        {
            this.innerFileProvider = innerFileProvider ?? throw new ArgumentNullException(nameof(innerFileProvider));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            if (options == null) throw new ArgumentNullException(nameof(options));
            this.rewriters = rewriters ?? new ISpaStaticFilesUrlRewriter[]
            {
                new HtmlUrlRewriter(),
                new ServiceWorkerJsUrlRewriter(),
            };
            this.sourcePathBase = options.SourcePathBase ?? "./";
            this.sourcePathBaseSelector = options.SourcePathBaseSelector;
            this.targetPathBaseSelector = options.TargetPathBaseSelector;
            this.maxFileLengthForRewrite = options.MaxFileLengthForRewrite;
        }

        /// <inheritdoc />
        public IDirectoryContents GetDirectoryContents(string subpath)
            => innerFileProvider.GetDirectoryContents(subpath);

        /// <inheritdoc />
        public IChangeToken Watch(string filter)
            => innerFileProvider.Watch(filter);

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            var fileInfo = innerFileProvider.GetFileInfo(subpath);
            fileInfo = RewriteFileIfNecessary(fileInfo);
            return fileInfo;
        }

        private IFileInfo RewriteFileIfNecessary(IFileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.IsDirectory || !fileInfo.Exists)
            {
                return fileInfo;
            }

            if (maxFileLengthForRewrite.HasValue)
            {
                if (fileInfo.Length <= 0 || fileInfo.Length > maxFileLengthForRewrite.Value)
                {
                    return fileInfo;
                }
            }

            var rewriter = rewriters.FirstOrDefault(rw => rw.CanRewriteFile(fileInfo.Name));
            if (rewriter == null)
            {
                return fileInfo;
            }

            var request = httpContextAccessor.HttpContext?.Request;
            var sourcePathBase = GetSourcePathBase(request);
            var targetPathBase = GetTargetPathBase(request);
            if (string.IsNullOrEmpty(sourcePathBase) || string.IsNullOrEmpty(targetPathBase) || sourcePathBase == targetPathBase)
            {
                return fileInfo;
            }

            string content;
            using (var stream = fileInfo.CreateReadStream())
            using (var streamReader = new StreamReader(stream))
            {
                content = streamReader.ReadToEnd();
            }

            string modifiedContent = rewriter.Rewrite(content, sourcePathBase, targetPathBase);
            bool isModified = content != modifiedContent;
            return new TextFileInfo(
                content: modifiedContent,
                name: fileInfo.Name,
                physicalPath: fileInfo.PhysicalPath,
                lastModified: isModified ? DateTimeOffset.UtcNow : fileInfo.LastModified
            );
        }

        private string GetSourcePathBase(HttpRequest request)
        {
            if (sourcePathBaseSelector != null)
            {
                return sourcePathBaseSelector(request);
            }
            return sourcePathBase;
        }

        private string GetTargetPathBase(HttpRequest request)
        {
            if (targetPathBaseSelector != null)
            {
                return targetPathBaseSelector(request);
            }

            string pathBaseString;
            pathBaseString = request?.PathBase.Value ?? string.Empty;
            pathBaseString = AddLeadingSlashIfNecessary(pathBaseString);
            pathBaseString = AddTrailingSlashIfNecessary(pathBaseString);
            return pathBaseString;
        }

        private static string AddLeadingSlashIfNecessary(string path)
        {
            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                path = '/' + path;
            }
            return path;
        }

        private static string AddTrailingSlashIfNecessary(string path)
        {
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path += '/';
            }
            return path;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (innerFileProvider is IDisposable disposableFileProvider)
            {
                disposableFileProvider.Dispose();
            }
        }
    }

    /// <summary>
    /// An implementation of <see cref="IFileInfo"/> that uses a string for its content.
    /// </summary>
    internal sealed class TextFileInfo : IFileInfo
    {
        private readonly string content;

        public TextFileInfo(string content, string name, string physicalPath, DateTimeOffset lastModified)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.PhysicalPath = physicalPath;
            this.LastModified = lastModified;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Stream CreateReadStream() => new MemoryStream(Encoding.GetBytes(content));

        /// <inheritdoc />
        public long Length => Encoding.GetByteCount(content);

        /// <summary>
        /// The encoding to use for <see cref="CreateReadStream()"/> and <see cref="Length"/>.
        /// </summary>
        /// <remarks>
        /// This could be turned into a class property to support different encodings.
        /// </remarks>
        private static Encoding Encoding => Encoding.UTF8;

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public string PhysicalPath { get; }

        /// <inheritdoc />
        public DateTimeOffset LastModified { get; }
    }
}
