using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// An alternative implementation of <see cref="SpaStaticFilesOptions"/> with additional options for <see cref="UrlWriteSpaStaticFileProvider"/>.
    /// </summary>
    public class UrlRewriteSpaStaticFilesOptions
    {
        /// <summary>
        /// The path, relative to the application root, of the directory in which the physical files are located.
        /// If the specified directory does not exist, then the <see cref="SpaStaticFilesExtensions.UseSpaStaticFiles(IApplicationBuilder)"/> middleware will not serve any static files.
        /// </summary>
        public string? RootPath { get; set; }

        /// <summary>
        /// The base path to replace in the source files.
        /// </summary>
        public string? SourcePathBase { get; set; }

        /// <summary>
        /// Selects the <see cref="SourcePathBase"/> for an HTTP request.
        /// Use this instead of <see cref="SourcePathBase "/> if the source path to replace is not the same for all files.
        /// </summary>
        public Func<HttpRequest?, string>? SourcePathBaseSelector { get; set; }

        /// <summary>
        /// Selects the replacement for the <see cref="SourcePathBase"/> from an HTTP request.
        /// </summary>
        public Func<HttpRequest?, string>? TargetPathBaseSelector { get; set; }

        /// <summary>
        /// The maximum length (in bytes) of files to include in the rewrite process.
        /// </summary>
        public int? MaxFileLengthForRewrite { get; set; }

        /// <summary>
        /// For HTML files, true if for only the HTML base element's href attribute should be updated and all other href/src attributes should be left alone.
        /// </summary>
        public bool? UpdateBaseElementHrefOnly { get; set; }

        /// <summary>
        /// Any custom rewriters to apply to static files.
        /// </summary>
        public IReadOnlyCollection<ISpaStaticFilesUrlRewriter>? Rewriters { get; set; }
    }
}
