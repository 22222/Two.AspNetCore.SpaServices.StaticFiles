using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Two.AspNetCore.SpaServices.StaticFiles.Mocks;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class SpaStaticFilesWithUrlRewriteExtensionsTest
    {
        [Fact]
        public void UseSpa_ShouldNotThrowException()
        {
            var services = GetServiceCollection();
            services.AddSpaStaticFilesWithUrlRewrite(configuration =>
            {
                configuration.RootPath = "Root";
            });

            var serviceProvider = services.BuildServiceProvider();
            var applicationBuilder = new MockApplicationBuilder(serviceProvider);
            applicationBuilder.UseSpa(x => { });
        }

        [Fact]
        public void UseSpa_RootPathNotSet_ShouldThrowInvalidOperationException()
        {
            var services = GetServiceCollection();
            services.AddSpaStaticFilesWithUrlRewrite(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var applicationBuilder = new MockApplicationBuilder(serviceProvider);
            var exception = Assert.Throws<InvalidOperationException>(
                () => applicationBuilder.UseSpa(_ => { }));
            Assert.Contains(nameof(UrlRewriteSpaStaticFilesOptions.RootPath), exception.Message, StringComparison.Ordinal);
        }

        private ServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.TryAddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment, MockWebHostEnvironment>();
            services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, MockHttpContextAccessor>();
            return services;
        }
    }
}
