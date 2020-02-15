using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System;

namespace Two.AspNetCore.SpaServices.StaticFiles.Mocks
{
    public class MockWebHostEnvironment : IWebHostEnvironment
    {
        /// <inheritdoc />
        public string EnvironmentName { get; set; } = "Mock";

        /// <inheritdoc />
        public string ApplicationName { get; set; } = string.Empty;

        /// <inheritdoc />
        public string WebRootPath { get; set; } = string.Empty;

        /// <inheritdoc />
        public IFileProvider? WebRootFileProvider { get; set; }

        /// <inheritdoc />
        public string ContentRootPath { get; set; } = string.Empty;

        /// <inheritdoc />
        public IFileProvider? ContentRootFileProvider { get; set; }
    }
}
