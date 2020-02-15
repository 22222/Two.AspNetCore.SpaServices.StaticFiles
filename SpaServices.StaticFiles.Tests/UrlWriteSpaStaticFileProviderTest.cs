using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Two.AspNetCore.SpaServices.StaticFiles.Mocks;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class UrlWriteSpaStaticFileProviderTest
    {
        [Fact]
        public void Construct_NullArguments_ShouldThrowArgumentNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new UrlWriteSpaStaticFileProvider(null, null));
            Assert.Throws<ArgumentNullException>(() => new UrlWriteSpaStaticFileProvider(GetServiceCollection().BuildServiceProvider(), null));
            Assert.Throws<ArgumentNullException>(() => new UrlWriteSpaStaticFileProvider(null, new UrlRewriteSpaStaticFilesOptions { RootPath = "ClientApp/build/" }));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void Construct_MissingRootPathArguments_ShouldThrowArgumentException()
        {
            var serviceProvider = GetServiceCollection().BuildServiceProvider();
            var options = new UrlRewriteSpaStaticFilesOptions();
            var ex = Assert.Throws<ArgumentException>(() => new UrlWriteSpaStaticFileProvider(serviceProvider, options));
            Assert.Equal("options", ex.ParamName);
        }

        [Fact]
        public void Construct_MissingServiceDependencies_ShouldThrowInvalidOperationException()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var options = new UrlRewriteSpaStaticFilesOptions
            {
                RootPath = "ClientApp/build",
            };
            Assert.Throws<InvalidOperationException>(() => new UrlWriteSpaStaticFileProvider(serviceProvider, options));
        }

        [Fact]
        public void Construct_RequiredParameters_ShouldNotThrowException()
        {
            var serviceProvider = GetServiceCollection().BuildServiceProvider();
            var options = new UrlRewriteSpaStaticFilesOptions
            {
                RootPath = "ClientApp/build",
            };
            using var actual = new UrlWriteSpaStaticFileProvider(serviceProvider, options);
            Assert.NotNull(actual);
        }

        private static ServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.TryAddSingleton<Microsoft.AspNetCore.Hosting.IHostingEnvironment, MockHostingEnvironment>();
            services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, MockHttpContextAccessor>();
            return services;
        }
    }
}
