using Microsoft.AspNetCore.Http;
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
    /// An implementation of <see cref="IFileProvider"/> that can rewrite URLs in the file content.
    /// </summary>
    internal sealed class UrlRewriteFileProvider : IFileProvider, IDisposable
    {
        private const string DefaultSourcePathBase = "./";

        private readonly IFileProvider innerFileProvider;
        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IReadOnlyCollection<ISpaStaticFilesUrlRewriter> rewriters;
        private readonly string sourcePathBase;
        private readonly Func<HttpRequest?, string>? sourcePathBaseSelector;
        private readonly Func<HttpRequest?, string>? targetPathBaseSelector;
        private readonly int? maxFileLengthForRewrite;

        public UrlRewriteFileProvider(IFileProvider innerFileProvider, IHttpContextAccessor httpContextAccessor, UrlRewriteSpaStaticFilesOptions? options)
        {
            this.innerFileProvider = innerFileProvider ?? throw new ArgumentNullException(nameof(innerFileProvider));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            if (options == null) options = new UrlRewriteSpaStaticFilesOptions();

            this.sourcePathBase = options.SourcePathBase ?? DefaultSourcePathBase;
            this.sourcePathBaseSelector = options.SourcePathBaseSelector;
            this.targetPathBaseSelector = options.TargetPathBaseSelector;
            this.maxFileLengthForRewrite = options.MaxFileLengthForRewrite;
            this.rewriters = CreateRewriterCollection(options);
        }

        private static IReadOnlyCollection<ISpaStaticFilesUrlRewriter> CreateRewriterCollection(UrlRewriteSpaStaticFilesOptions options)
        {
            var defaultRewriters = new ISpaStaticFilesUrlRewriter[]
            {
                new HtmlUrlRewriter() { UpdateBaseElementHrefOnly = options.UpdateBaseElementHrefOnly },
                new ServiceWorkerJsUrlRewriter(),
            };

            var customRewriters = options.Rewriters;
            if (customRewriters == null || !customRewriters.Any())
            {
                return defaultRewriters;
            }

            var rewriterList = new List<ISpaStaticFilesUrlRewriter>(defaultRewriters.Length + customRewriters.Count);
            rewriterList.AddRange(customRewriters);
            rewriterList.AddRange(defaultRewriters);
            return rewriterList;
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
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (fileInfo.IsDirectory || !fileInfo.Exists)
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
            if (string.IsNullOrEmpty(sourcePathBase) || string.IsNullOrEmpty(targetPathBase) || string.Equals(sourcePathBase, targetPathBase, StringComparison.Ordinal))
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
                physicalPath: isModified ? null : fileInfo.PhysicalPath,
                lastModified: isModified ? DateTimeOffset.UtcNow : fileInfo.LastModified
            );
        }

        private string GetSourcePathBase(HttpRequest? request)
        {
            if (sourcePathBaseSelector != null)
            {
                return sourcePathBaseSelector(request);
            }
            return sourcePathBase;
        }

        private string GetTargetPathBase(HttpRequest? request)
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
            if (path.Length == 0 || path[0] != '/')
            {
                path = '/' + path;
            }

            return path;
        }

        private static string AddTrailingSlashIfNecessary(string path)
        {
            if (path.Length == 0 || path[path.Length - 1] != '/')
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

        public TextFileInfo(string content, string name, string? physicalPath, DateTimeOffset lastModified)
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
        public string? PhysicalPath { get; }

        /// <inheritdoc />
        public DateTimeOffset LastModified { get; }
    }
}
