using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class ServiceWorkerJsUrlRewriterTest
    {
        private readonly ServiceWorkerJsUrlRewriter rewriter = new ServiceWorkerJsUrlRewriter();

        [Theory]
        [InlineData("service-worker.js", true)]
        [InlineData("serviceWorker.js", true)]
        [InlineData("test-service-worker.js", true)]
        [InlineData("test-serviceWorker.js", true)]
        [InlineData("test-sw.js", true)]
        [InlineData("test.js", false)]
        [InlineData("precache-manifest.js", true)]
        [InlineData("precache-manifest.22222222222222222222222222222222.js", true)]
        [InlineData("index-precache-manifest.js", true)]
        [InlineData("index-precache-manifest.22222222222222222222222222222222.js", true)]
        [InlineData("_.js", false)]
        [InlineData(".js", false)]
        [InlineData("test.html", false)]
        [InlineData("test.txt", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void CanRewriteFile(string name, bool expected)
        {
            var actual = rewriter.CanRewriteFile(name);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("sample1")]
        [InlineData("sample2")]
        [InlineData("sample3")]
        [InlineData("precache-manifest_sample1")]
        [InlineData("precache-manifest_sample2")]
        public void Rewrite_SampleInputShouldMatchExpected(string testName)
        {
            var input = TestFileSource.ReadEmbeddedTextFile("ServiceWorkerJsRewriter", $"{testName}_input.js");
            var expected = TestFileSource.ReadEmbeddedTextFile("ServiceWorkerJsRewriter", $"{testName}_expected.js");

            var actual = rewriter.Rewrite(input, sourcePathBase: "./", targetPathBase: "/path/to/");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Rewrite_ThrowsArgumentNullException_IfJsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => rewriter.Rewrite(null, "./", "path/to"));
        }

        [Fact]
        public void Rewrite_ReturnsInputJs_IfSourceOrTargetIsNullOrEmpty()
        {
            var js = @"importScripts(""./test.js"")";
            Assert.Equal(js, rewriter.Rewrite(js, null, null));
            Assert.Equal(js, rewriter.Rewrite(js, string.Empty, string.Empty));
            Assert.Equal(js, rewriter.Rewrite(js, "./", null));
            Assert.Equal(js, rewriter.Rewrite(js, "./", string.Empty));
            Assert.Equal(js, rewriter.Rewrite(js, null, "./"));
            Assert.Equal(js, rewriter.Rewrite(js, string.Empty, "./"));
        }

        [Fact]
        public void Rewrite_ShouldHandleSpecialCharacters()
        {
            var js = @"importScripts(""./*/test.js"")";
            var expected = @"importScripts(""/$1/test.js"")";
            var actual = rewriter.Rewrite(js, "./*/", "/$1/");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Rewrite_PrecacheManifest_ShouldHandleSpecialCharacters()
        {
            var js = @"self.__precacheManifest = [{url:""./*/test.js""}]";
            var expected = @"self.__precacheManifest = [{url:""/$1/test.js""}]";
            var actual = rewriter.Rewrite(js, "./*/", "/$1/");
            Assert.Equal(expected, actual);
        }
    }
}
