namespace Two.AspNetCore.SpaServices.StaticFiles
{
    /// <summary>
    /// Rewrites the file contents of an SPA static file.
    /// </summary>
    public interface ISpaStaticFilesUrlRewriter
    {
        /// <summary>
        /// Returns true if this rewriter can rewrite the specified file.
        /// </summary>
        /// <param name="name">The name of the file to rewrite.</param>
        /// <returns>True if the file can be rewritten, or false if it cannot.</returns>
        bool CanRewriteFile(string name);

        /// <summary>
        /// Rewrites the specified string content.
        /// </summary>
        /// <param name="content">The original content.</param>
        /// <param name="sourcePathBase">The source path.</param>
        /// <param name="targetPathBase">The target path that should be used to replace the source path.</param>
        /// <returns>The modified content.</returns>
        string Rewrite(string content, string sourcePathBase, string targetPathBase);
    }
}
