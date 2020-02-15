using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class HtmlUrlRewriterTest
    {
        private readonly HtmlUrlRewriter rewriter = new HtmlUrlRewriter();

        [Theory]
        [InlineData("test.html", true)]
        [InlineData("_.html", true)]
        [InlineData(".html", true)]
        [InlineData("test.htm", false)]
        [InlineData("test.js", false)]
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
        public void Rewrite_SampleInputShouldMatchExpected(string testName)
        {
            var input = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_input.html");
            var expected = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_expected.html");

            var actual = rewriter.Rewrite(input, sourcePathBase: "./", targetPathBase: "/path/to/");
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("sample1")]
        [InlineData("sample2")]
        public void Rewrite_SampleInputShouldMatchExpected_WhenUpdateBaseElementHrefOnly(string testName)
        {
            var input = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_input.html");
            var expected = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_expected_baseonly.html");

            rewriter.UpdateBaseElementHrefOnly = true;
            var actual = rewriter.Rewrite(input, sourcePathBase: "./", targetPathBase: "/path/to/");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Rewrite_ThrowsArgumentNullException_IfHtmlIsNull()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => rewriter.Rewrite(null, "./", "path/to"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void Rewrite_ReturnsInputHtml_IfSourceOrTargetIsNullOrEmpty()
        {
            var html = @"<html src=""./test.html""></html>";

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Equal(html, rewriter.Rewrite(html, null, null));
            Assert.Equal(html, rewriter.Rewrite(html, string.Empty, string.Empty));
            Assert.Equal(html, rewriter.Rewrite(html, "./", null));
            Assert.Equal(html, rewriter.Rewrite(html, "./", string.Empty));
            Assert.Equal(html, rewriter.Rewrite(html, null, "./"));
            Assert.Equal(html, rewriter.Rewrite(html, string.Empty, "./"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void Rewrite_ShouldHandleSpecialCharacters()
        {
            var html = @"<html src=""./*/test/test.html""></html>";
            var expected = @"<html src=""/$1/test/test.html""></html>";
            var actual = rewriter.Rewrite(html, "./*/", "/$1/");
            Assert.Equal(expected, actual);
        }
    }
}
