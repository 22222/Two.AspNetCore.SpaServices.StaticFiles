using System.IO;
using System.Linq;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    internal static class TestFileSource
    {
        public static string ReadEmbeddedTextFile(string resourceCategory, string resourcePath)
        {
            var resourceName = $"{typeof(TestFileSource).Namespace}.TestFiles.{resourceCategory}.{resourcePath}";
            string content;
            using (var scriptStream = ReadEmbeddedFileStream(resourceName))
            using (var scriptReader = new StreamReader(scriptStream))
            {
                content = scriptReader.ReadToEnd();
            }
            return content;
        }

        private static Stream ReadEmbeddedFileStream(string resourceName)
        {
            var assembly = typeof(TestFileSource).Assembly;
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var sampleNames = assembly.GetManifestResourceNames().Take(3);
                throw new FileNotFoundException($"No resource found for name <{resourceName}> in assembly <{assembly.FullName}>.  Sample names: [{string.Join(", ", sampleNames)}]");
            }
            return stream;
        }
    }
}
