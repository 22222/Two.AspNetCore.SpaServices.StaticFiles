using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Two.AspNetCore.SpaServices.StaticFiles.Mocks
{
    public class MockFileInfo : IFileInfo
    {
        private readonly byte[] contentBytes;

        public MockFileInfo(byte[] contentBytes, string name, string? physicalPath, DateTimeOffset lastModified)
        {
            this.contentBytes = contentBytes ?? throw new ArgumentNullException(nameof(contentBytes));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PhysicalPath = physicalPath ?? throw new ArgumentNullException(nameof(physicalPath));
            LastModified = lastModified;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Stream CreateReadStream() => new MemoryStream(contentBytes);

        /// <inheritdoc />
        public long Length => contentBytes.Length;

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
