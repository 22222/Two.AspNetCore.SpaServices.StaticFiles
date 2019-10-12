using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// Rewrites paths in service worker files.
    /// </summary>
    public class ServiceWorkerJsUrlRewriter : ISpaStaticFilesUrlRewriter
    {
        /// <summary>
        /// Returns true if the file has a "js" extension and its name contains "service-worker", "serviceWorker", "-sw", or "precache-manifest".
        /// </summary>
        /// <inheritdoc />
        public virtual bool CanRewriteFile(string name)
        {
            if (name == null) return false;

            bool isJsFile = name.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
            if (!isJsFile)
            {
                return false;
            }

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            bool isServiceWorkerName = nameWithoutExtension.Equals("service-worker", StringComparison.OrdinalIgnoreCase)
                || nameWithoutExtension.Equals("serviceWorker", StringComparison.OrdinalIgnoreCase)
                || nameWithoutExtension.EndsWith("-service-worker", StringComparison.OrdinalIgnoreCase)
                || nameWithoutExtension.EndsWith("-serviceWorker", StringComparison.OrdinalIgnoreCase)
                || nameWithoutExtension.EndsWith("-sw", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("precache-manifest", StringComparison.OrdinalIgnoreCase)
                || name.IndexOf("-precache-manifest", StringComparison.OrdinalIgnoreCase) >= 0;
            return isServiceWorkerName;
        }

        /// <inheritdoc />
        public virtual string Rewrite(string js, string sourcePathBase, string targetPathBase)
        {
            if (js == null) throw new ArgumentNullException(nameof(js));
            if (string.IsNullOrEmpty(sourcePathBase) || string.IsNullOrEmpty(targetPathBase) || sourcePathBase == targetPathBase)
            {
                return js;
            }

            var modifiedJs = js;
            modifiedJs = Regex.Replace(modifiedJs, $@"([[(=:]\s*[""']){Regex.Escape(sourcePathBase)}", $"$1{RegexEscapeReplacement(targetPathBase)}", RegexOptions.IgnoreCase);
            return modifiedJs;
        }

        private static string RegexEscapeReplacement(string replacement) => replacement?.Replace("$", "$$");
    }
}
