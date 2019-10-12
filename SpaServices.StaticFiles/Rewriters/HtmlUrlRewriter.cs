using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// Rewrites paths in HTML files.
    /// </summary>
    public class HtmlUrlRewriter : ISpaStaticFilesUrlRewriter
    {
        /// <summary>
        /// True if only the HTML base element's href attribute should be updated and all other href/src attributes should be left alone.
        /// </summary>
        public bool? UpdateBaseElementHrefOnly { get; set; }

        /// <summary>
        /// Returns true if the file has an "html" extension.
        /// </summary>
        /// <inheritdoc />
        public virtual bool CanRewriteFile(string name)
        {
            if (name == null) return false;

            bool isHtmlFile = name.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
            return isHtmlFile;
        }

        /// <inheritdoc />
        public virtual string Rewrite(string html, string sourcePathBase, string targetPathBase)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (string.IsNullOrEmpty(sourcePathBase) || string.IsNullOrEmpty(targetPathBase) || sourcePathBase == targetPathBase)
            {
                return html;
            }

            string modifiedHtml;
            if (UpdateBaseElementHrefOnly == true)
            {
                modifiedHtml = Regex.Replace(html, $@"(<base\s*href\s*=\s*""){Regex.Escape(sourcePathBase)}", $"$1{RegexEscapeReplacement(targetPathBase)}", RegexOptions.IgnoreCase);
            }
            else
            {
                modifiedHtml = Regex.Replace(html, $@"((?:^|\s)(?:href|src)\s*=\s*""){Regex.Escape(sourcePathBase)}", $@"$1{RegexEscapeReplacement(targetPathBase)}", RegexOptions.IgnoreCase);
            }
            return modifiedHtml;
        }

        private static string RegexEscapeReplacement(string replacement) => replacement?.Replace("$", "$$");
    }
}
