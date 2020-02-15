using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Two.AspNetCore.SpaServices.StaticFiles.Mocks;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class UrlRewriteFileProviderTest
    {
        private readonly MockFileProvider mockFileProvider = new MockFileProvider();
        private readonly MockHttpContextAccessor mockHttpContextAccessor = new MockHttpContextAccessor();

        [Fact]
        public void GetFileInfo_FileDoesNotExist()
        {
            var options = new UrlRewriteSpaStaticFilesOptions();
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("test.html");

            Assert.Equal("test.html", actual.Name);
            Assert.False(actual.Exists);
        }

        [Fact]
        public void GetFileInfo_MinimalSampleHtmlFile_NoChanges()
        {
            mockFileProvider.SetFileInfo("test.html", "<html />");

            var options = new UrlRewriteSpaStaticFilesOptions();
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("test.html");

            Assert.Equal("test.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal("<html />", actualText);

            Assert.Equal(8, actual.Length);
            Assert.EndsWith("test.html", actual.PhysicalPath, StringComparison.Ordinal);
            Assert.Equal(MockFileProvider.DefaultModifiedTime, actual.LastModified);
        }

        [Theory]
        [InlineData("sample1")]
        [InlineData("sample2")]
        public void GetFileInfo_SampleHtmlInputShouldMatchExpected(string testName)
        {
            var inputText = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_input.html");
            var expectedText = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_expected.html");

            mockFileProvider.SetFileInfo("index.html", inputText);
            mockHttpContextAccessor.PathBase = "/path/to/";

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                SourcePathBase = "./",
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("index.html");

            Assert.Equal("index.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal(expectedText, actualText);

            Assert.Null(actual.PhysicalPath);
            Assert.True(actual.LastModified > MockFileProvider.DefaultModifiedTime);
            Assert.Equal(actual.Length, Encoding.UTF8.GetBytes(expectedText).Length);
        }

        [Theory]
        [InlineData("sample1")]
        [InlineData("sample2")]
        public void GetFileInfo_SampleHtmlInputShouldMatchExpected_UpdateBaseElementHrefOnly(string testName)
        {
            var inputText = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_input.html");
            var expectedText = TestFileSource.ReadEmbeddedTextFile("HtmlRewriter", $"{testName}_expected_baseonly.html");

            mockFileProvider.SetFileInfo("index.html", inputText);
            mockHttpContextAccessor.PathBase = "/path/to/";

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                SourcePathBase = "./",
                UpdateBaseElementHrefOnly = true,
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("index.html");

            Assert.Equal("index.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal(expectedText, actualText);

            if (inputText != expectedText)
            {
                Assert.Null(actual.PhysicalPath);
                Assert.True(actual.LastModified > MockFileProvider.DefaultModifiedTime);
            }
            else
            {
                Assert.NotNull(actual.PhysicalPath);
                Assert.Equal(MockFileProvider.DefaultModifiedTime, actual.LastModified);
            }
            Assert.Equal(actual.Length, Encoding.UTF8.GetBytes(expectedText).Length);
        }

        [Theory]
        [InlineData("sample1")]
        [InlineData("sample2")]
        [InlineData("sample3")]
        [InlineData("precache-manifest_sample1")]
        [InlineData("precache-manifest_sample2")]
        public void GetFileInfo_SampleServiceWorkerJsInputShouldMatchExpected(string testName)
        {
            var inputText = TestFileSource.ReadEmbeddedTextFile("ServiceWorkerJsRewriter", $"{testName}_input.js");
            var expectedText = TestFileSource.ReadEmbeddedTextFile("ServiceWorkerJsRewriter", $"{testName}_expected.js");

            mockFileProvider.SetFileInfo("service-worker.js", inputText);
            mockHttpContextAccessor.PathBase = "/path/to/";

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                SourcePathBase = "./",
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("service-worker.js");

            Assert.Equal("service-worker.js", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal(expectedText, actualText);

            Assert.Null(actual.PhysicalPath);
            Assert.True(actual.LastModified > MockFileProvider.DefaultModifiedTime);
            Assert.Equal(actual.Length, Encoding.UTF8.GetBytes(expectedText).Length);
        }

        [Fact]
        public void GetFileInfo_MinimalSampleHtmlFile_SourceAndTargetPathBaseSelectors()
        {
            mockFileProvider.SetFileInfo("test.html", "<base href=\"./root/path/\" />");
            mockHttpContextAccessor.PathBase = "/virtual";

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                SourcePathBaseSelector = (request) => "./root/",
                TargetPathBaseSelector = (request) => (request?.PathBase ?? string.Empty) + "/newroot/",
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("test.html");

            Assert.Equal("test.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal("<base href=\"/virtual/newroot/path/\" />", actualText);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void GetFileInfo_MinimalSampleHtmlFile_MaxFileLengthForRewrite(int maxInputLengthOffset)
        {
            var inputText = "<base href=\"./\" />";
            var inputLength = Encoding.UTF8.GetBytes(inputText).Length;

            mockFileProvider.SetFileInfo("test.html", inputText);
            mockHttpContextAccessor.PathBase = "/virtual";

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                SourcePathBase = "./",
                MaxFileLengthForRewrite = inputLength + maxInputLengthOffset,
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("test.html");

            Assert.Equal("test.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();

            if (maxInputLengthOffset < 0)
            {
                Assert.Equal(inputText, actualText);

                Assert.NotNull(actual.PhysicalPath);
                Assert.Equal(MockFileProvider.DefaultModifiedTime, actual.LastModified);
                Assert.Equal(actual.Length, inputLength);
            }
            else
            {
                Assert.Equal("<base href=\"/virtual/\" />", actualText);

                Assert.Null(actual.PhysicalPath);
                Assert.True(actual.LastModified > MockFileProvider.DefaultModifiedTime);
                Assert.True(actual.Length > inputLength);
            }
        }

        [Fact]
        public void GetFileInfo_MinimalSampleHtmlFile_CustomRewriter()
        {
            mockFileProvider.SetFileInfo("test.html", "<html />");

            var options = new UrlRewriteSpaStaticFilesOptions
            {
                Rewriters = new[]
                {
                    new HelloWorldRewriter(),
                },
            };
            using var urlRewriteFileProvider = new UrlRewriteFileProvider(mockFileProvider, mockHttpContextAccessor, options);
            var actual = urlRewriteFileProvider.GetFileInfo("test.html");

            Assert.Equal("test.html", actual.Name);
            Assert.True(actual.Exists);

            using var actualReadStream = actual.CreateReadStream();
            using var actualReader = new StreamReader(actualReadStream);
            var actualText = actualReader.ReadToEnd();
            Assert.Equal("Hello, world!", actualText);

            Assert.Equal(13, actual.Length);
            Assert.Null(actual.PhysicalPath);
            Assert.True(actual.LastModified > MockFileProvider.DefaultModifiedTime);
        }

        private class HelloWorldRewriter : ISpaStaticFilesUrlRewriter
        {
            public bool CanRewriteFile(string name) => true;

            public string Rewrite(string content, string sourcePathBase, string targetPathBase) => "Hello, world!";
        }
    }
}
