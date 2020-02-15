using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Two.AspNetCore.SpaServices.StaticFiles.Mocks
{
    public class MockFileProvider : IFileProvider
    {
        public static readonly DateTimeOffset DefaultModifiedTime = new DateTimeOffset(2000, 2, 2, 12, 0, 0, TimeSpan.Zero);

        private ConcurrentDictionary<string, MockFileInfo> files = new ConcurrentDictionary<string, MockFileInfo>();

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            if (!files.TryGetValue(subpath, out var mockFileInfo))
            {
                return new NotFoundFileInfo(subpath);
            }

            return mockFileInfo;
        }

        public void SetFileInfo(string subpath, string contentText)
        {
            var contentBytes = Encoding.UTF8.GetBytes(contentText);
            SetFileInfo(subpath, contentBytes);
        }

        public void SetFileInfo(string subpath, byte[] contentBytes)
        {
            var fileInfo = new MockFileInfo(contentBytes, subpath, physicalPath: Path.Combine(@"mock://", subpath), lastModified: DefaultModifiedTime);
            files[subpath] = fileInfo;
        }

        /// <inheritdoc />
        IDirectoryContents IFileProvider.GetDirectoryContents(string subpath)
            => throw new NotSupportedException();

        /// <inheritdoc />
        IChangeToken IFileProvider.Watch(string filter)
            => throw new NotSupportedException();
    }
}
